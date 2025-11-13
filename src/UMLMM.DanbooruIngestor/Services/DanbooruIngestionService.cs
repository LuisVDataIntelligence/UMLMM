using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UMLMM.Domain.Entities;
using UMLMM.DanbooruIngestor.Configuration;
using UMLMM.DanbooruIngestor.Danbooru;
using UMLMM.DanbooruIngestor.Mapping;
using UMLMM.Infrastructure.Data;

namespace UMLMM.DanbooruIngestor.Services;

public class DanbooruIngestionService
{
    private readonly IDanbooruApiClient _apiClient;
    private readonly UmlmmDbContext _dbContext;
    private readonly DanbooruMapper _mapper;
    private readonly DanbooruSettings _settings;
    private readonly ILogger<DanbooruIngestionService> _logger;

    public DanbooruIngestionService(
        IDanbooruApiClient apiClient,
        UmlmmDbContext dbContext,
        DanbooruMapper mapper,
        DanbooruSettings settings,
        ILogger<DanbooruIngestionService> logger)
    {
        _apiClient = apiClient;
        _dbContext = dbContext;
        _mapper = mapper;
        _settings = settings;
        _logger = logger;
    }

    public async Task<FetchRun> IngestPostsAsync(string runId, CancellationToken cancellationToken)
    {
        var fetchRun = new FetchRun
        {
            Id = Guid.NewGuid(),
            RunId = runId,
            SourceId = "danbooru",
            StartedAt = DateTime.UtcNow,
            Parameters = System.Text.Json.JsonSerializer.Serialize(new
            {
                _settings.Tags,
                _settings.PageSize,
                _settings.MaxPages
            })
        };

        _dbContext.FetchRuns.Add(fetchRun);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Starting ingestion run {RunId} for Danbooru", runId);

        try
        {
            for (int page = 1; page <= _settings.MaxPages; page++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Ingestion cancelled at page {Page}", page);
                    break;
                }

                try
                {
                    var posts = await _apiClient.GetPostsAsync(
                        page, 
                        _settings.PageSize, 
                        _settings.Tags, 
                        cancellationToken);

                    if (posts.Count == 0)
                    {
                        _logger.LogInformation("No more posts found at page {Page}, stopping", page);
                        break;
                    }

                    foreach (var post in posts)
                    {
                        try
                        {
                            await UpsertPostAsync(post, fetchRun, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error upserting post {PostId}", post.Id);
                            fetchRun.ErrorCount++;
                        }
                    }

                    _logger.LogInformation(
                        "Processed page {Page}/{MaxPages}: Created={Created}, Updated={Updated}, NoOp={NoOp}, Errors={Errors}",
                        page, _settings.MaxPages, fetchRun.CreatedCount, fetchRun.UpdatedCount, 
                        fetchRun.NoOpCount, fetchRun.ErrorCount);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching page {Page}", page);
                    fetchRun.ErrorCount++;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error during ingestion run {RunId}", runId);
            fetchRun.ErrorDetails = ex.ToString();
        }
        finally
        {
            fetchRun.CompletedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation(
                "Completed ingestion run {RunId}: Duration={Duration}s, Created={Created}, Updated={Updated}, NoOp={NoOp}, Errors={Errors}",
                runId,
                (fetchRun.CompletedAt.Value - fetchRun.StartedAt).TotalSeconds,
                fetchRun.CreatedCount,
                fetchRun.UpdatedCount,
                fetchRun.NoOpCount,
                fetchRun.ErrorCount);
        }

        return fetchRun;
    }

    private async Task UpsertPostAsync(DanbooruPostDto post, FetchRun fetchRun, CancellationToken cancellationToken)
    {
        var image = _mapper.MapToImage(post);
        var tagInfos = _mapper.ExtractTags(post);

        // Check if image exists
        var existingImage = await _dbContext.Images
            .Include(i => i.ImageTags)
            .ThenInclude(it => it.Tag)
            .FirstOrDefaultAsync(
                i => i.SourceId == image.SourceId && i.ExternalId == image.ExternalId,
                cancellationToken);

        if (existingImage != null)
        {
            // Check if anything changed
            var hasChanges = existingImage.Sha256 != image.Sha256 ||
                            existingImage.PreviewUrl != image.PreviewUrl ||
                            existingImage.OriginalUrl != image.OriginalUrl ||
                            existingImage.Rating != image.Rating;

            if (hasChanges)
            {
                existingImage.Sha256 = image.Sha256;
                existingImage.PreviewUrl = image.PreviewUrl;
                existingImage.OriginalUrl = image.OriginalUrl;
                existingImage.Rating = image.Rating;
                existingImage.Metadata = image.Metadata;
                existingImage.UpdatedAt = DateTime.UtcNow;
                fetchRun.UpdatedCount++;
            }
            else
            {
                fetchRun.NoOpCount++;
            }

            image = existingImage;
        }
        else
        {
            image.Id = Guid.NewGuid();
            _dbContext.Images.Add(image);
            await _dbContext.SaveChangesAsync(cancellationToken);
            fetchRun.CreatedCount++;
        }

        // Upsert tags
        await UpsertTagsAsync(image, tagInfos, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task UpsertTagsAsync(
        Image image, 
        List<(string Name, string? Category)> tagInfos, 
        CancellationToken cancellationToken)
    {
        var tagNames = tagInfos.Select(t => t.Name).ToList();
        var existingTags = await _dbContext.Tags
            .Where(t => tagNames.Contains(t.Name))
            .ToDictionaryAsync(t => t.Name, cancellationToken);

        var existingImageTags = await _dbContext.ImageTags
            .Where(it => it.ImageId == image.Id)
            .ToListAsync(cancellationToken);

        foreach (var (name, category) in tagInfos)
        {
            // Get or create tag
            if (!existingTags.TryGetValue(name, out var tag))
            {
                tag = new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Category = category,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _dbContext.Tags.Add(tag);
                await _dbContext.SaveChangesAsync(cancellationToken);
                existingTags[name] = tag;
            }

            // Create image-tag relationship if it doesn't exist
            if (!existingImageTags.Any(it => it.TagId == tag.Id))
            {
                var imageTag = new ImageTag
                {
                    ImageId = image.Id,
                    TagId = tag.Id,
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.ImageTags.Add(imageTag);
            }
        }
    }
}
