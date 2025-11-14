using System.Text.Json;
using Microsoft.Extensions.Logging;
using UMLMM.Core.Entities;

namespace UMLMM.Infrastructure.Repositories;

public class JsonModelRepository : IModelRepository
{
    private readonly string _filePath;
    private readonly ILogger<JsonModelRepository> _logger;
    private readonly object _sync = new object();

    public JsonModelRepository(string filePath, ILogger<JsonModelRepository> logger)
    {
        _filePath = string.IsNullOrWhiteSpace(filePath) ? "umlmm.models.json" : filePath;
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

    public Task<Source> UpsertSourceAsync(string name, string? description = null, CancellationToken cancellationToken = default)
    {
        var store = LoadData();
        var source = store.Sources.FirstOrDefault(s => s.Name == name);

        if (source == null)
        {
            source = new Source
            {
                Id = store.NextId(),
                Name = name,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            store.Sources.Add(source);
        }
        else
        {
            source.Description = description;
            source.UpdatedAt = DateTime.UtcNow;
        }

        SaveData(store);
        return Task.FromResult(source);
    }

    public Task<Model> UpsertModelAsync(int sourceId, string externalId, string name, string? description = null, System.Text.Json.JsonDocument? metadata = null, CancellationToken cancellationToken = default)
    {
        var store = LoadData();
        var model = store.Models.FirstOrDefault(m => m.SourceId == sourceId && m.ExternalId == externalId);

        if (model == null)
        {
            model = new Model
            {
                Id = store.NextId(),
                SourceId = sourceId,
                ExternalId = externalId,
                Name = name,
                Description = description,
                Metadata = metadata,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            store.Models.Add(model);
        }
        else
        {
            model.Name = name;
            model.Description = description;
            model.Metadata = metadata;
            model.UpdatedAt = DateTime.UtcNow;
        }

        SaveData(store);
        return Task.FromResult(model);
    }

    public Task<ModelVersion> UpsertModelVersionAsync(int modelId, string tag, string externalId, string? parentModel = null, System.Text.Json.JsonDocument? parameters = null, System.Text.Json.JsonDocument? metadata = null, CancellationToken cancellationToken = default)
    {
        var store = LoadData();
        var version = store.ModelVersions.FirstOrDefault(v => v.ModelId == modelId && v.Tag == tag);

        if (version == null)
        {
            version = new ModelVersion
            {
                Id = store.NextId(),
                ModelId = modelId,
                Tag = tag,
                ExternalId = externalId,
                ParentModel = parentModel,
                Parameters = parameters,
                Metadata = metadata,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            store.ModelVersions.Add(version);
        }
        else
        {
            version.ExternalId = externalId;
            version.ParentModel = parentModel;
            version.Parameters = parameters;
            version.Metadata = metadata;
            version.UpdatedAt = DateTime.UtcNow;
        }

        SaveData(store);
        return Task.FromResult(version);
    }

    public Task<ModelArtifact> UpsertModelArtifactAsync(int modelVersionId, string type, string? digest = null, long? size = null, string? mediaType = null, System.Text.Json.JsonDocument? metadata = null, CancellationToken cancellationToken = default)
    {
        var store = LoadData();
        var artifact = store.ModelArtifacts.FirstOrDefault(a => a.ModelVersionId == modelVersionId && a.Type == type && (digest == null || a.Digest == digest));

        if (artifact == null)
        {
            artifact = new ModelArtifact
            {
                Id = store.NextId(),
                ModelVersionId = modelVersionId,
                Type = type,
                Digest = digest,
                Size = size,
                MediaType = mediaType,
                Metadata = metadata,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            store.ModelArtifacts.Add(artifact);
        }
        else
        {
            artifact.Digest = digest;
            artifact.Size = size;
            artifact.MediaType = mediaType;
            artifact.Metadata = metadata;
            artifact.UpdatedAt = DateTime.UtcNow;
        }

        SaveData(store);
        return Task.FromResult(artifact);
    }

    public Task<FetchRun> CreateFetchRunAsync(int sourceId, Guid runId, CancellationToken cancellationToken = default)
    {
        var store = LoadData();
        var fetchRun = new FetchRun
        {
            Id = store.NextId(),
            SourceId = sourceId,
            RunId = runId,
            StartedAt = DateTime.UtcNow,
            Status = "running"
        };
        store.FetchRuns.Add(fetchRun);
        SaveData(store);
        return Task.FromResult(fetchRun);
    }

    public Task UpdateFetchRunAsync(int fetchRunId, string status, int modelsProcessed, int versionsProcessed, int artifactsProcessed, string? errorMessage = null, CancellationToken cancellationToken = default)
    {
        var store = LoadData();
        var fetchRun = store.FetchRuns.FirstOrDefault(f => f.Id == fetchRunId);
        if (fetchRun == null)
        {
            throw new InvalidOperationException($"FetchRun with ID {fetchRunId} not found");
        }

        fetchRun.Status = status;
        fetchRun.ModelsProcessed = modelsProcessed;
        fetchRun.VersionsProcessed = versionsProcessed;
        fetchRun.ArtifactsProcessed = artifactsProcessed;
        fetchRun.ErrorMessage = errorMessage;

        if (status == "completed" || status == "failed")
        {
            fetchRun.CompletedAt = DateTime.UtcNow;
        }

        SaveData(store);
        return Task.CompletedTask;
    }

    private class JsonStore
    {
        public List<Source> Sources { get; set; } = new();
        public List<Model> Models { get; set; } = new();
        public List<ModelVersion> ModelVersions { get; set; } = new();
        public List<ModelArtifact> ModelArtifacts { get; set; } = new();
        public List<FetchRun> FetchRuns { get; set; } = new();

        private int _lastId = 0;

        public int NextId()
        {
            _lastId++;
            return _lastId;
        }
    }
}
