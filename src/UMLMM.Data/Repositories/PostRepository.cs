using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UMLMM.Core.Domain.Entities;

namespace UMLMM.Data.Repositories;

public class PostRepository : IPostRepository
{
    private readonly UmlmmDbContext _context;
    private readonly ILogger<PostRepository> _logger;

    public PostRepository(UmlmmDbContext context, ILogger<PostRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Source> EnsureSourceAsync(string sourceName, CancellationToken cancellationToken = default)
    {
        var source = await _context.Sources
            .FirstOrDefaultAsync(s => s.Name == sourceName, cancellationToken);

        if (source == null)
        {
            source = new Source
            {
                Name = sourceName,
                CreatedAt = DateTime.UtcNow
            };
            _context.Sources.Add(source);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Created new source: {SourceName} with ID {SourceId}", sourceName, source.Id);
        }

        return source;
    }

    public async Task<FetchRun> CreateFetchRunAsync(int sourceId, CancellationToken cancellationToken = default)
    {
        var fetchRun = new FetchRun
        {
            SourceId = sourceId,
            StartedAt = DateTime.UtcNow,
            PostsFetched = 0,
            PostsCreated = 0,
            PostsUpdated = 0,
            Success = false
        };

        _context.FetchRuns.Add(fetchRun);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Created fetch run {FetchRunId} for source {SourceId}", fetchRun.Id, sourceId);
        
        return fetchRun;
    }

    public async Task UpdateFetchRunAsync(FetchRun fetchRun, CancellationToken cancellationToken = default)
    {
        _context.FetchRuns.Update(fetchRun);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Post?> GetPostByExternalIdAsync(int sourceId, string externalId, CancellationToken cancellationToken = default)
    {
        return await _context.Posts
            .Include(p => p.PostTags)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.SourceId == sourceId && p.ExternalId == externalId, cancellationToken);
    }

    public async Task<Tag?> GetTagByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
    }

    public async Task UpsertPostAsync(
        Post post, 
        List<Tag> tags, 
        Image image, 
        CancellationToken cancellationToken = default)
    {
        var existingPost = await GetPostByExternalIdAsync(post.SourceId, post.ExternalId, cancellationToken);

        if (existingPost != null)
        {
            // Update existing post
            existingPost.Description = post.Description;
            existingPost.Rating = post.Rating;
            existingPost.ExternalCreatedAt = post.ExternalCreatedAt;
            existingPost.UpdatedAt = DateTime.UtcNow;

            // Update or add image
            var existingImage = existingPost.Images.FirstOrDefault();
            if (existingImage != null)
            {
                existingImage.Url = image.Url;
                existingImage.SampleUrl = image.SampleUrl;
                existingImage.Width = image.Width;
                existingImage.Height = image.Height;
                existingImage.FileSize = image.FileSize;
                existingImage.FileExtension = image.FileExtension;
                existingImage.Sha256 = image.Sha256;
            }
            else
            {
                image.PostId = existingPost.Id;
                _context.Images.Add(image);
            }

            // Update tags - remove old ones and add new ones
            var existingPostTags = existingPost.PostTags.ToList();
            foreach (var pt in existingPostTags)
            {
                _context.PostTags.Remove(pt);
            }
        }
        else
        {
            // Create new post
            _context.Posts.Add(post);
            await _context.SaveChangesAsync(cancellationToken);
            
            image.PostId = post.Id;
            _context.Images.Add(image);
        }

        // Ensure all tags exist and link them to the post
        var postId = existingPost?.Id ?? post.Id;
        foreach (var tag in tags)
        {
            var existingTag = await GetTagByNameAsync(tag.Name, cancellationToken);
            
            if (existingTag == null)
            {
                _context.Tags.Add(tag);
                await _context.SaveChangesAsync(cancellationToken);
                existingTag = tag;
            }

            var postTag = new PostTag
            {
                PostId = postId,
                TagId = existingTag.Id
            };
            
            _context.PostTags.Add(postTag);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
