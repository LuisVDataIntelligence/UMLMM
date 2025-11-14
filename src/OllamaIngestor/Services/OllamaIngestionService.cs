using System.Text.Json;
using UMLMM.Infrastructure.Repositories;
using OllamaIngestor.Models;

namespace OllamaIngestor.Services;

public class OllamaIngestionService
{
    private readonly IOllamaClient _ollamaClient;
    private readonly IModelRepository _modelRepository;
    private readonly ILogger<OllamaIngestionService> _logger;

    public OllamaIngestionService(
        IOllamaClient ollamaClient,
        IModelRepository modelRepository,
        ILogger<OllamaIngestionService> logger)
    {
        _ollamaClient = ollamaClient;
        _modelRepository = modelRepository;
        _logger = logger;
    }

    public async Task<(int models, int versions, int artifacts)> IngestModelsAsync(
        int sourceId, 
        Guid runId, 
        CancellationToken cancellationToken = default)
    {
        int modelsProcessed = 0;
        int versionsProcessed = 0;
        int artifactsProcessed = 0;

        try
        {
            _logger.LogInformation("Starting Ollama model ingestion. RunId: {RunId}", runId);

            var ollamaModels = await _ollamaClient.ListModelsAsync(cancellationToken);
            _logger.LogInformation("Found {Count} Ollama models", ollamaModels.Count);

            foreach (var ollamaModel in ollamaModels)
            {
                try
                {
                    var result = await IngestSingleModelAsync(
                        sourceId, 
                        runId, 
                        ollamaModel,
                        cancellationToken);
                    
                    modelsProcessed += result.models;
                    versionsProcessed += result.versions;
                    artifactsProcessed += result.artifacts;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to ingest model {ModelName}. RunId: {RunId}", ollamaModel.Name, runId);
                }
            }

            _logger.LogInformation(
                "Completed Ollama model ingestion. RunId: {RunId}, Models: {Models}, Versions: {Versions}, Artifacts: {Artifacts}",
                runId, modelsProcessed, versionsProcessed, artifactsProcessed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Ollama model ingestion. RunId: {RunId}", runId);
            throw;
        }

        return (modelsProcessed, versionsProcessed, artifactsProcessed);
    }

    private async Task<(int models, int versions, int artifacts)> IngestSingleModelAsync(
        int sourceId,
        Guid runId,
        OllamaModel ollamaModel,
        CancellationToken cancellationToken)
    {
        int modelsProcessed = 0;
        int versionsProcessed = 0;
        int artifactsProcessed = 0;

        // Parse model name (format: name:tag or just name)
        var (modelName, tag) = ParseModelName(ollamaModel.Name);

        _logger.LogInformation("Ingesting model {ModelName}:{Tag}. RunId: {RunId}", modelName, tag, runId);

        // Get detailed model information
        var modelShow = await _ollamaClient.ShowModelAsync(ollamaModel.Name, cancellationToken);

        // Create metadata from model details
        var metadata = new Dictionary<string, object>();
        if (ollamaModel.Size.HasValue)
            metadata["size"] = ollamaModel.Size.Value;
        if (ollamaModel.Digest != null)
            metadata["digest"] = ollamaModel.Digest;
        if (ollamaModel.ModifiedAt != null)
            metadata["modified_at"] = ollamaModel.ModifiedAt;
        if (ollamaModel.Details != null)
        {
            foreach (var kvp in ollamaModel.Details)
            {
                metadata[$"details_{kvp.Key}"] = kvp.Value;
            }
        }

        var metadataJson = JsonSerializer.SerializeToDocument(metadata);

        // Upsert model
        var model = await _modelRepository.UpsertModelAsync(
            sourceId,
            modelName,
            modelName,
            description: null,
            metadata: metadataJson,
            cancellationToken);

        modelsProcessed++;

        // Extract parent model from modelfile if available
        string? parentModel = null;
        if (modelShow?.Modelfile != null)
        {
            parentModel = ExtractParentModel(modelShow.Modelfile);
        }

        // Create parameters JSON
        JsonDocument? parametersJson = null;
        if (modelShow?.Parameters != null)
        {
            parametersJson = JsonSerializer.SerializeToDocument(modelShow.Parameters);
        }

        // Create version metadata
        var versionMetadata = new Dictionary<string, object>();
        if (modelShow?.Template != null)
            versionMetadata["template"] = modelShow.Template;
        if (modelShow?.Details != null)
        {
            foreach (var kvp in modelShow.Details)
            {
                versionMetadata[$"details_{kvp.Key}"] = kvp.Value;
            }
        }

        var versionMetadataJson = versionMetadata.Count > 0 
            ? JsonSerializer.SerializeToDocument(versionMetadata) 
            : null;

        // Upsert model version
        var version = await _modelRepository.UpsertModelVersionAsync(
            model.Id,
            tag,
            ollamaModel.Name,
            parentModel,
            parametersJson,
            versionMetadataJson,
            cancellationToken);

        versionsProcessed++;

        // Create artifact for modelfile
        if (modelShow?.Modelfile != null)
        {
            var modelfileMetadata = JsonSerializer.SerializeToDocument(new Dictionary<string, object>
            {
                ["content"] = modelShow.Modelfile
            });

            await _modelRepository.UpsertModelArtifactAsync(
                version.Id,
                "modelfile",
                digest: ollamaModel.Digest,
                size: ollamaModel.Size,
                mediaType: "text/plain",
                metadata: modelfileMetadata,
                cancellationToken);

            artifactsProcessed++;
        }

        _logger.LogInformation(
            "Successfully ingested model {ModelName}:{Tag} (ModelId: {ModelId}, VersionId: {VersionId}). RunId: {RunId}",
            modelName, tag, model.Id, version.Id, runId);

        return (modelsProcessed, versionsProcessed, artifactsProcessed);
    }

    private static (string name, string tag) ParseModelName(string fullName)
    {
        var parts = fullName.Split(':', 2);
        return parts.Length == 2 
            ? (parts[0], parts[1]) 
            : (fullName, "latest");
    }

    private static string? ExtractParentModel(string modelfile)
    {
        // Parse FROM line in modelfile
        // Example: FROM llama2:7b
        var lines = modelfile.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var line in lines)
        {
            if (line.StartsWith("FROM ", StringComparison.OrdinalIgnoreCase))
            {
                var parts = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    return parts[1].Trim();
                }
            }
        }
        return null;
    }
}
