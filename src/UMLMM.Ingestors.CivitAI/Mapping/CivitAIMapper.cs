using System.Text.Json;
using System.Text.RegularExpressions;
using UMLMM.Domain.Entities;
using UMLMM.Ingestors.CivitAI.CivitAI.DTOs;

namespace UMLMM.Ingestors.CivitAI.Mapping;

public static class CivitAIMapper
{
    public static Model MapToModel(CivitAIModelDto dto, int sourceId)
    {
        var model = new Model
        {
            SourceId = sourceId,
            ExternalId = dto.Id.ToString(),
            Name = dto.Name,
            Type = dto.Type,
            Description = dto.Description,
            Nsfw = dto.Nsfw,
            Raw = JsonSerializer.Serialize(dto),
            UpdatedAt = DateTime.UtcNow
        };

        // Map versions
        if (dto.ModelVersions != null)
        {
            foreach (var versionDto in dto.ModelVersions)
            {
                var version = MapToVersion(versionDto, model);
                model.Versions.Add(version);
            }
        }

        return model;
    }

    public static ModelVersion MapToVersion(CivitAIVersionDto dto, Model? model = null)
    {
        var version = new ModelVersion
        {
            ExternalId = dto.Id.ToString(),
            VersionLabel = dto.Name,
            PublishedAt = dto.PublishedAt,
            Raw = JsonSerializer.Serialize(dto),
            UpdatedAt = DateTime.UtcNow
        };

        if (model != null)
        {
            version.Model = model;
        }

        // Map files to artifacts
        if (dto.Files != null)
        {
            foreach (var fileDto in dto.Files)
            {
                var artifact = MapToArtifact(fileDto, version);
                version.Artifacts.Add(artifact);
            }
        }

        // Map images
        if (dto.Images != null)
        {
            foreach (var imageDto in dto.Images)
            {
                var image = MapToImage(imageDto, version);
                version.Images.Add(image);
            }
        }

        return version;
    }

    public static Artifact MapToArtifact(CivitAIFileDto dto, ModelVersion? version = null)
    {
        var artifact = new Artifact
        {
            ExternalId = dto.Id.ToString(),
            FileKind = dto.Type,
            FileSizeBytes = dto.SizeKB.HasValue ? dto.SizeKB.Value * 1024 : null,
            Sha256 = dto.Hashes?.SHA256,
            DownloadUrl = dto.DownloadUrl,
            Raw = JsonSerializer.Serialize(dto),
            UpdatedAt = DateTime.UtcNow
        };

        if (version != null)
        {
            artifact.Version = version;
        }

        return artifact;
    }

    public static Image MapToImage(CivitAIImageDto dto, ModelVersion? version = null)
    {
        var image = new Image
        {
            ExternalId = dto.Id.ToString(),
            PreviewUrl = dto.Url,
            Width = dto.Width,
            Height = dto.Height,
            Rating = dto.NsfwLevel,
            Raw = JsonSerializer.Serialize(dto),
            UpdatedAt = DateTime.UtcNow
        };

        if (version != null)
        {
            image.Version = version;
        }

        return image;
    }

    public static List<string> NormalizeTags(List<string>? tags)
    {
        if (tags == null || tags.Count == 0)
            return new List<string>();

        return tags
            .Select(NormalizeTag)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct()
            .ToList();
    }

    public static string NormalizeTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return string.Empty;

        // Convert to lowercase
        var normalized = tag.ToLowerInvariant().Trim();

        // Replace spaces and underscores with hyphens
        normalized = Regex.Replace(normalized, @"[\s_]+", "-");

        // Remove any characters that aren't alphanumeric or hyphens
        normalized = Regex.Replace(normalized, @"[^a-z0-9-]", "");

        // Remove multiple consecutive hyphens
        normalized = Regex.Replace(normalized, @"-+", "-");

        // Remove leading/trailing hyphens
        normalized = normalized.Trim('-');

        return normalized;
    }
}
