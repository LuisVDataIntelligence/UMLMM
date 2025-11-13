using Microsoft.EntityFrameworkCore;
using UMLMM.Core.Entities;
using UMLMM.Infrastructure.Data;

namespace UMLMM.Infrastructure.Repositories;

public class ModelRepository
{
    private readonly UmlmmDbContext _context;

    public ModelRepository(UmlmmDbContext context)
    {
        _context = context;
    }

    public async Task<Source> UpsertSourceAsync(string name, string? description = null, CancellationToken cancellationToken = default)
    {
        var source = await _context.Sources.FirstOrDefaultAsync(s => s.Name == name, cancellationToken);
        
        if (source == null)
        {
            source = new Source 
            { 
                Name = name, 
                Description = description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Sources.Add(source);
        }
        else
        {
            source.Description = description;
            source.UpdatedAt = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        return source;
    }

    public async Task<Model> UpsertModelAsync(int sourceId, string externalId, string name, string? description = null, System.Text.Json.JsonDocument? metadata = null, CancellationToken cancellationToken = default)
    {
        var model = await _context.Models
            .FirstOrDefaultAsync(m => m.SourceId == sourceId && m.ExternalId == externalId, cancellationToken);
        
        if (model == null)
        {
            model = new Model
            {
                SourceId = sourceId,
                ExternalId = externalId,
                Name = name,
                Description = description,
                Metadata = metadata,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Models.Add(model);
        }
        else
        {
            model.Name = name;
            model.Description = description;
            model.Metadata = metadata;
            model.UpdatedAt = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        return model;
    }

    public async Task<ModelVersion> UpsertModelVersionAsync(
        int modelId, 
        string tag, 
        string externalId, 
        string? parentModel = null, 
        System.Text.Json.JsonDocument? parameters = null, 
        System.Text.Json.JsonDocument? metadata = null, 
        CancellationToken cancellationToken = default)
    {
        var version = await _context.ModelVersions
            .FirstOrDefaultAsync(v => v.ModelId == modelId && v.Tag == tag, cancellationToken);
        
        if (version == null)
        {
            version = new ModelVersion
            {
                ModelId = modelId,
                Tag = tag,
                ExternalId = externalId,
                ParentModel = parentModel,
                Parameters = parameters,
                Metadata = metadata,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.ModelVersions.Add(version);
        }
        else
        {
            version.ExternalId = externalId;
            version.ParentModel = parentModel;
            version.Parameters = parameters;
            version.Metadata = metadata;
            version.UpdatedAt = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        return version;
    }

    public async Task<ModelArtifact> UpsertModelArtifactAsync(
        int modelVersionId,
        string type,
        string? digest = null,
        long? size = null,
        string? mediaType = null,
        System.Text.Json.JsonDocument? metadata = null,
        CancellationToken cancellationToken = default)
    {
        // Find existing artifact by version and type (and digest if provided)
        var query = _context.ModelArtifacts
            .Where(a => a.ModelVersionId == modelVersionId && a.Type == type);
        
        if (!string.IsNullOrEmpty(digest))
        {
            query = query.Where(a => a.Digest == digest);
        }
        
        var artifact = await query.FirstOrDefaultAsync(cancellationToken);
        
        if (artifact == null)
        {
            artifact = new ModelArtifact
            {
                ModelVersionId = modelVersionId,
                Type = type,
                Digest = digest,
                Size = size,
                MediaType = mediaType,
                Metadata = metadata,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.ModelArtifacts.Add(artifact);
        }
        else
        {
            artifact.Digest = digest;
            artifact.Size = size;
            artifact.MediaType = mediaType;
            artifact.Metadata = metadata;
            artifact.UpdatedAt = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        return artifact;
    }

    public async Task<FetchRun> CreateFetchRunAsync(int sourceId, Guid runId, CancellationToken cancellationToken = default)
    {
        var fetchRun = new FetchRun
        {
            SourceId = sourceId,
            RunId = runId,
            StartedAt = DateTime.UtcNow,
            Status = "running"
        };
        
        _context.FetchRuns.Add(fetchRun);
        await _context.SaveChangesAsync(cancellationToken);
        return fetchRun;
    }

    public async Task UpdateFetchRunAsync(
        int fetchRunId,
        string status,
        int modelsProcessed,
        int versionsProcessed,
        int artifactsProcessed,
        string? errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        var fetchRun = await _context.FetchRuns.FindAsync(new object[] { fetchRunId }, cancellationToken);
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
        
        await _context.SaveChangesAsync(cancellationToken);
    }
}
