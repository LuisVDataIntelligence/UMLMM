using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UMLMM.Domain.Entities;
using UMLMM.Infrastructure.Data;
using UMLMM.Ingestors.CivitAI.CivitAI.Client;
using UMLMM.Ingestors.CivitAI.Mapping;

namespace UMLMM.Ingestors.CivitAI.Services;

public class CivitAIIngestionService
{
    private readonly CivitAIApiClient _apiClient;
    private readonly UmlmmDbContext _dbContext;
    private readonly ILogger<CivitAIIngestionService> _logger;
    private const string SourceName = "CivitAI";
    private const string SourceType = "model-repository";

    public CivitAIIngestionService(
        CivitAIApiClient apiClient,
        UmlmmDbContext dbContext,
        ILogger<CivitAIIngestionService> logger)
    {
        _apiClient = apiClient;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<FetchRun> IngestAsync(
        int startPage = 1,
        int pageSize = 100,
        int? maxPages = null,
        string? apiKey = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting CivitAI ingestion: startPage={StartPage}, pageSize={PageSize}, maxPages={MaxPages}",
            startPage, pageSize, maxPages);

        // Get or create source
        var source = await GetOrCreateSourceAsync(cancellationToken);

        // Create fetch run
        var fetchRun = new FetchRun
        {
            SourceId = source.Id,
            StartedAt = DateTime.UtcNow,
            Status = "running",
            Cursor = startPage.ToString()
        };
        _dbContext.FetchRuns.Add(fetchRun);
        await _dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            var currentPage = startPage;
            var pageCount = 0;

            while (true)
            {
                if (maxPages.HasValue && pageCount >= maxPages.Value)
                {
                    _logger.LogInformation("Reached maximum page limit: {MaxPages}", maxPages.Value);
                    break;
                }

                try
                {
                    var response = await _apiClient.GetModelsAsync(currentPage, pageSize, apiKey, cancellationToken);

                    if (response.Items.Count == 0)
                    {
                        _logger.LogInformation("No more models to fetch");
                        break;
                    }

                    // Process each model
                    foreach (var modelDto in response.Items)
                    {
                        try
                        {
                            await UpsertModelAsync(modelDto, source.Id, fetchRun, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing model {ModelId}", modelDto.Id);
                            fetchRun.RecordsError++;
                        }
                    }

                    // Save progress after each page
                    fetchRun.Cursor = currentPage.ToString();
                    await _dbContext.SaveChangesAsync(cancellationToken);

                    pageCount++;
                    currentPage++;

                    // Check if we've reached the last page
                    if (response.Metadata?.TotalPages.HasValue == true && currentPage > response.Metadata.TotalPages.Value)
                    {
                        _logger.LogInformation("Reached last page: {TotalPages}", response.Metadata.TotalPages.Value);
                        break;
                    }

                    // Add a small delay to be respectful of rate limits
                    await Task.Delay(100, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching page {Page}", currentPage);
                    throw;
                }
            }

            fetchRun.Status = "completed";
            fetchRun.CompletedAt = DateTime.UtcNow;
            _logger.LogInformation("Ingestion completed: Created={Created}, Updated={Updated}, NoOp={NoOp}, Errors={Errors}",
                fetchRun.RecordsCreated, fetchRun.RecordsUpdated, fetchRun.RecordsNoOp, fetchRun.RecordsError);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ingestion failed");
            fetchRun.Status = "failed";
            fetchRun.CompletedAt = DateTime.UtcNow;
            fetchRun.ErrorMessage = ex.Message;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return fetchRun;
    }

    private async Task<Source> GetOrCreateSourceAsync(CancellationToken cancellationToken)
    {
        var source = await _dbContext.Sources
            .FirstOrDefaultAsync(s => s.Name == SourceName, cancellationToken);

        if (source == null)
        {
            source = new Source
            {
                Name = SourceName,
                Type = SourceType,
                BaseUrl = "https://civitai.com",
                IsActive = true
            };
            _dbContext.Sources.Add(source);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Created new source: {SourceName}", SourceName);
        }

        return source;
    }

    private async Task UpsertModelAsync(
        CivitAI.DTOs.CivitAIModelDto modelDto,
        int sourceId,
        FetchRun fetchRun,
        CancellationToken cancellationToken)
    {
        var externalId = modelDto.Id.ToString();

        // Check if model exists
        var existingModel = await _dbContext.Models
            .Include(m => m.Versions)
                .ThenInclude(v => v.Artifacts)
            .Include(m => m.Versions)
                .ThenInclude(v => v.Images)
            .Include(m => m.ModelTags)
            .FirstOrDefaultAsync(m => m.SourceId == sourceId && m.ExternalId == externalId, cancellationToken);

        if (existingModel == null)
        {
            // Create new model
            var newModel = CivitAIMapper.MapToModel(modelDto, sourceId);
            _dbContext.Models.Add(newModel);

            // Process tags
            if (modelDto.Tags != null && modelDto.Tags.Count > 0)
            {
                var normalizedTags = CivitAIMapper.NormalizeTags(modelDto.Tags);
                await AssignTagsToModelAsync(newModel, normalizedTags, cancellationToken);
            }

            fetchRun.RecordsCreated++;
            _logger.LogDebug("Created model {ExternalId}: {Name}", externalId, modelDto.Name);
        }
        else
        {
            // Check if model needs update (compare based on version count or other criteria)
            var hasChanges = false;

            if (existingModel.Name != modelDto.Name ||
                existingModel.Description != modelDto.Description ||
                existingModel.Nsfw != modelDto.Nsfw ||
                existingModel.Type != modelDto.Type)
            {
                existingModel.Name = modelDto.Name;
                existingModel.Description = modelDto.Description;
                existingModel.Nsfw = modelDto.Nsfw;
                existingModel.Type = modelDto.Type;
                existingModel.UpdatedAt = DateTime.UtcNow;
                hasChanges = true;
            }

            // Upsert versions
            if (modelDto.ModelVersions != null)
            {
                foreach (var versionDto in modelDto.ModelVersions)
                {
                    var versionChanged = await UpsertVersionAsync(existingModel, versionDto, cancellationToken);
                    hasChanges = hasChanges || versionChanged;
                }
            }

            // Update tags
            if (modelDto.Tags != null && modelDto.Tags.Count > 0)
            {
                var normalizedTags = CivitAIMapper.NormalizeTags(modelDto.Tags);
                var tagsChanged = await UpdateTagsForModelAsync(existingModel, normalizedTags, cancellationToken);
                hasChanges = hasChanges || tagsChanged;
            }

            if (hasChanges)
            {
                fetchRun.RecordsUpdated++;
                _logger.LogDebug("Updated model {ExternalId}: {Name}", externalId, modelDto.Name);
            }
            else
            {
                fetchRun.RecordsNoOp++;
            }
        }
    }

    private async Task<bool> UpsertVersionAsync(
        Model model,
        CivitAI.DTOs.CivitAIVersionDto versionDto,
        CancellationToken cancellationToken)
    {
        var versionExternalId = versionDto.Id.ToString();
        var existingVersion = model.Versions.FirstOrDefault(v => v.ExternalId == versionExternalId);

        if (existingVersion == null)
        {
            var newVersion = CivitAIMapper.MapToVersion(versionDto);
            newVersion.ModelId = model.Id;
            model.Versions.Add(newVersion);
            return true;
        }

        var hasChanges = false;

        if (existingVersion.VersionLabel != versionDto.Name ||
            existingVersion.PublishedAt != versionDto.PublishedAt)
        {
            existingVersion.VersionLabel = versionDto.Name;
            existingVersion.PublishedAt = versionDto.PublishedAt;
            existingVersion.UpdatedAt = DateTime.UtcNow;
            hasChanges = true;
        }

        // Upsert artifacts
        if (versionDto.Files != null)
        {
            foreach (var fileDto in versionDto.Files)
            {
                var artifactChanged = await UpsertArtifactAsync(existingVersion, fileDto, cancellationToken);
                hasChanges = hasChanges || artifactChanged;
            }
        }

        // Upsert images
        if (versionDto.Images != null)
        {
            foreach (var imageDto in versionDto.Images)
            {
                var imageChanged = await UpsertImageAsync(existingVersion, imageDto, cancellationToken);
                hasChanges = hasChanges || imageChanged;
            }
        }

        return hasChanges;
    }

    private async Task<bool> UpsertArtifactAsync(
        ModelVersion version,
        CivitAI.DTOs.CivitAIFileDto fileDto,
        CancellationToken cancellationToken)
    {
        var artifactExternalId = fileDto.Id.ToString();
        var sha256 = fileDto.Hashes?.SHA256;

        // Check by external_id first
        var existingArtifact = version.Artifacts.FirstOrDefault(a => a.ExternalId == artifactExternalId);

        // If not found by external_id and sha256 is available, check by sha256
        if (existingArtifact == null && !string.IsNullOrEmpty(sha256))
        {
            existingArtifact = version.Artifacts.FirstOrDefault(a => a.Sha256 == sha256);
        }

        if (existingArtifact == null)
        {
            var newArtifact = CivitAIMapper.MapToArtifact(fileDto);
            newArtifact.VersionId = version.Id;
            version.Artifacts.Add(newArtifact);
            return true;
        }

        // Update if needed
        var hasChanges = false;
        if (existingArtifact.FileKind != fileDto.Type ||
            existingArtifact.DownloadUrl != fileDto.DownloadUrl)
        {
            existingArtifact.FileKind = fileDto.Type;
            existingArtifact.DownloadUrl = fileDto.DownloadUrl;
            existingArtifact.UpdatedAt = DateTime.UtcNow;
            hasChanges = true;
        }

        return hasChanges;
    }

    private async Task<bool> UpsertImageAsync(
        ModelVersion version,
        CivitAI.DTOs.CivitAIImageDto imageDto,
        CancellationToken cancellationToken)
    {
        var imageExternalId = imageDto.Id.ToString();
        var existingImage = version.Images.FirstOrDefault(i => i.ExternalId == imageExternalId);

        if (existingImage == null)
        {
            var newImage = CivitAIMapper.MapToImage(imageDto);
            newImage.VersionId = version.Id;
            version.Images.Add(newImage);
            return true;
        }

        // Update if needed
        var hasChanges = false;
        if (existingImage.PreviewUrl != imageDto.Url ||
            existingImage.Width != imageDto.Width ||
            existingImage.Height != imageDto.Height ||
            existingImage.Rating != imageDto.NsfwLevel)
        {
            existingImage.PreviewUrl = imageDto.Url;
            existingImage.Width = imageDto.Width;
            existingImage.Height = imageDto.Height;
            existingImage.Rating = imageDto.NsfwLevel;
            existingImage.UpdatedAt = DateTime.UtcNow;
            hasChanges = true;
        }

        return hasChanges;
    }

    private async Task AssignTagsToModelAsync(
        Model model,
        List<string> normalizedTags,
        CancellationToken cancellationToken)
    {
        foreach (var tagName in normalizedTags)
        {
            var tag = await _dbContext.Tags
                .FirstOrDefaultAsync(t => t.Name == tagName, cancellationToken);

            if (tag == null)
            {
                tag = new Tag { Name = tagName };
                _dbContext.Tags.Add(tag);
                await _dbContext.SaveChangesAsync(cancellationToken); // Save to get ID
            }

            model.ModelTags.Add(new ModelTag
            {
                Model = model,
                Tag = tag
            });
        }
    }

    private async Task<bool> UpdateTagsForModelAsync(
        Model model,
        List<string> normalizedTags,
        CancellationToken cancellationToken)
    {
        var currentTagNames = await _dbContext.ModelTags
            .Where(mt => mt.ModelId == model.Id)
            .Include(mt => mt.Tag)
            .Select(mt => mt.Tag.Name)
            .ToListAsync(cancellationToken);

        var tagsToAdd = normalizedTags.Except(currentTagNames).ToList();
        var tagsToRemove = currentTagNames.Except(normalizedTags).ToList();

        if (tagsToAdd.Count == 0 && tagsToRemove.Count == 0)
        {
            return false;
        }

        // Remove old tags
        if (tagsToRemove.Count > 0)
        {
            var modelTagsToRemove = await _dbContext.ModelTags
                .Where(mt => mt.ModelId == model.Id && tagsToRemove.Contains(mt.Tag.Name))
                .ToListAsync(cancellationToken);

            _dbContext.ModelTags.RemoveRange(modelTagsToRemove);
        }

        // Add new tags
        foreach (var tagName in tagsToAdd)
        {
            var tag = await _dbContext.Tags
                .FirstOrDefaultAsync(t => t.Name == tagName, cancellationToken);

            if (tag == null)
            {
                tag = new Tag { Name = tagName };
                _dbContext.Tags.Add(tag);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            _dbContext.ModelTags.Add(new ModelTag
            {
                ModelId = model.Id,
                TagId = tag.Id
            });
        }

        return true;
    }
}
