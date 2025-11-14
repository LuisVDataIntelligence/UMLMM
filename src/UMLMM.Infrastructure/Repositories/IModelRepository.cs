using UMLMM.Core.Entities;

namespace UMLMM.Infrastructure.Repositories;

public interface IModelRepository
{
    Task<Source> UpsertSourceAsync(string name, string? description = null, CancellationToken cancellationToken = default);
    Task<Model> UpsertModelAsync(int sourceId, string externalId, string name, string? description = null, System.Text.Json.JsonDocument? metadata = null, CancellationToken cancellationToken = default);
    Task<ModelVersion> UpsertModelVersionAsync(int modelId, string tag, string externalId, string? parentModel = null, System.Text.Json.JsonDocument? parameters = null, System.Text.Json.JsonDocument? metadata = null, CancellationToken cancellationToken = default);
    Task<ModelArtifact> UpsertModelArtifactAsync(int modelVersionId, string type, string? digest = null, long? size = null, string? mediaType = null, System.Text.Json.JsonDocument? metadata = null, CancellationToken cancellationToken = default);
    Task<FetchRun> CreateFetchRunAsync(int sourceId, Guid runId, CancellationToken cancellationToken = default);
    Task UpdateFetchRunAsync(int fetchRunId, string status, int modelsProcessed, int versionsProcessed, int artifactsProcessed, string? errorMessage = null, CancellationToken cancellationToken = default);
}
