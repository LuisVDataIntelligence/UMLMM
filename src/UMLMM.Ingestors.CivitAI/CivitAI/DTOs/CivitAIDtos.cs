using System.Text.Json.Serialization;

namespace UMLMM.Ingestors.CivitAI.CivitAI.DTOs;

public class CivitAIModelDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("nsfw")]
    public bool Nsfw { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    [JsonPropertyName("modelVersions")]
    public List<CivitAIVersionDto>? ModelVersions { get; set; }
}

public class CivitAIVersionDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("publishedAt")]
    public DateTime? PublishedAt { get; set; }

    [JsonPropertyName("files")]
    public List<CivitAIFileDto>? Files { get; set; }

    [JsonPropertyName("images")]
    public List<CivitAIImageDto>? Images { get; set; }
}

public class CivitAIFileDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("sizeKB")]
    public long? SizeKB { get; set; }

    [JsonPropertyName("hashes")]
    public CivitAIHashesDto? Hashes { get; set; }

    [JsonPropertyName("downloadUrl")]
    public string? DownloadUrl { get; set; }
}

public class CivitAIHashesDto
{
    [JsonPropertyName("SHA256")]
    public string? SHA256 { get; set; }
}

public class CivitAIImageDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("width")]
    public int? Width { get; set; }

    [JsonPropertyName("height")]
    public int? Height { get; set; }

    [JsonPropertyName("nsfwLevel")]
    public string? NsfwLevel { get; set; }
}

public class CivitAIModelsResponse
{
    [JsonPropertyName("items")]
    public List<CivitAIModelDto> Items { get; set; } = new();

    [JsonPropertyName("metadata")]
    public CivitAIMetadataDto? Metadata { get; set; }
}

public class CivitAIMetadataDto
{
    [JsonPropertyName("currentPage")]
    public int CurrentPage { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("totalPages")]
    public int? TotalPages { get; set; }

    [JsonPropertyName("totalItems")]
    public long? TotalItems { get; set; }

    [JsonPropertyName("nextCursor")]
    public string? NextCursor { get; set; }
}
