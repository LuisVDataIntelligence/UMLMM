using System.Text.Json;
using Microsoft.Extensions.Logging;
using UMLMM.Core.Domain.Entities;

namespace UMLMM.Data.Repositories;

public class JsonPostRepository : IPostRepository
{
    private readonly string _filePath;
    private readonly ILogger<JsonPostRepository> _logger;
    private readonly object _sync = new object();

    public JsonPostRepository(string filePath, ILogger<JsonPostRepository> logger)
    {
        _filePath = string.IsNullOrWhiteSpace(filePath) ? "umlmm.posts.json" : filePath;
        _logger = logger;

        if (!File.Exists(_filePath))
        {
            SaveData(new JsonStore());
        }
    }

    private JsonStore LoadData()
    {
        lock (_sync)
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<JsonStore>(json) ?? new JsonStore();
        }
    }

    private void SaveData(JsonStore store)
    {
        lock (_sync)
        {
            var json = JsonSerializer.Serialize(store, new JsonSerializerOptions{ WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
    }

    public Task<Source> EnsureSourceAsync(string sourceName, CancellationToken cancellationToken = default)
    {
        var store = LoadData();
        var source = store.Sources.FirstOrDefault(s => s.Name == sourceName);

        if (source == null)
        {
            source = new Source
            {
                Id = store.NextId(),
                Name = sourceName,
                CreatedAt = DateTime.UtcNow
            };
            store.Sources.Add(source);
            SaveData(store);
            _logger.LogInformation("Created new source {SourceName} id={SourceId}", sourceName, source.Id);
        }

        return Task.FromResult(source);
    }

    public Task<FetchRun> CreateFetchRunAsync(int sourceId, CancellationToken cancellationToken = default)
    {
        var store = LoadData();
        var fetchRun = new FetchRun
        {
            Id = store.NextId(),
            SourceId = sourceId,
            StartedAt = DateTime.UtcNow,
            PostsFetched = 0,
            PostsCreated = 0,
            PostsUpdated = 0,
            Success = false
        };
        store.FetchRuns.Add(fetchRun);
        SaveData(store);
        return Task.FromResult(fetchRun);
    }

    public Task UpdateFetchRunAsync(FetchRun fetchRun, CancellationToken cancellationToken = default)
    {
        var store = LoadData();
        var f = store.FetchRuns.FirstOrDefault(fr => fr.Id == fetchRun.Id);
        if (f != null)
        {
            f.PostsFetched = fetchRun.PostsFetched;
            f.PostsCreated = fetchRun.PostsCreated;
            f.PostsUpdated = fetchRun.PostsUpdated;
            f.Success = fetchRun.Success;
            f.CompletedAt = fetchRun.CompletedAt;
        }
        SaveData(store);
        return Task.CompletedTask;
    }

    public Task<Post?> GetPostByExternalIdAsync(int sourceId, string externalId, CancellationToken cancellationToken = default)
    {
        var store = LoadData();
        var post = store.Posts.FirstOrDefault(p => p.SourceId == sourceId && p.ExternalId == externalId);
        if (post == null) return Task.FromResult<Post?>(null);

        // Attach related tags and images
        post.PostTags ??= store.PostTags.Where(pt => pt.PostId == post.Id).ToList();
        post.Images ??= store.Images.Where(i => i.PostId == post.Id).ToList();
        return Task.FromResult<Post?>(post);
    }

    public Task<Tag?> GetTagByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var store = LoadData();
        var tag = store.Tags.FirstOrDefault(t => t.Name == name);
        return Task.FromResult(tag);
    }

    public Task UpsertPostAsync(Post post, List<Tag> tags, Image image, CancellationToken cancellationToken = default)
    {
        var store = LoadData();
        var existing = store.Posts.FirstOrDefault(p => p.SourceId == post.SourceId && p.ExternalId == post.ExternalId);

        if (existing != null)
        {
            existing.Description = post.Description;
            existing.Rating = post.Rating;
            existing.ExternalCreatedAt = post.ExternalCreatedAt;
            existing.UpdatedAt = DateTime.UtcNow;

            var existingImage = store.Images.FirstOrDefault(i => i.PostId == existing.Id);
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
                image.PostId = existing.Id;
                image.Id = store.NextId();
                store.Images.Add(image);
            }
        }
        else
        {
            post.Id = store.NextId();
            store.Posts.Add(post);

            image.PostId = post.Id;
            image.Id = store.NextId();
            store.Images.Add(image);
        }

        foreach (var tag in tags)
        {
            var existingTag = store.Tags.FirstOrDefault(t => t.Name == tag.Name);
            if (existingTag == null)
            {
                tag.Id = store.NextId();
                store.Tags.Add(tag);
                existingTag = tag;
            }

            var postTagExists = store.PostTags.Any(pt => pt.PostId == (existing?.Id ?? post.Id) && pt.TagId == existingTag.Id);
            if (!postTagExists)
            {
                store.PostTags.Add(new PostTag { PostId = existing?.Id ?? post.Id, TagId = existingTag.Id });
            }
        }

        SaveData(store);
        return Task.CompletedTask;
    }

    private class JsonStore
    {
        public List<Source> Sources { get; set; } = new();
        public List<FetchRun> FetchRuns { get; set; } = new();
        public List<Post> Posts { get; set; } = new();
        public List<Tag> Tags { get; set; } = new();
        public List<Image> Images { get; set; } = new();
        public List<PostTag> PostTags { get; set; } = new();

        private int _lastId = 0;
        public int NextId()
        {
            _lastId++;
            return _lastId;
        }
    }
}
