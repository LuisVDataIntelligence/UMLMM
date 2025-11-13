namespace Contracts.DTOs;

/// <summary>
/// Represents a model in the system
/// </summary>
public class ModelDto
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Model name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Model description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Source system (CivitAI, Danbooru, e621, ComfyUI, Ollama)
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// External ID from source system
    /// </summary>
    public string? ExternalId { get; set; }

    /// <summary>
    /// Model type (checkpoint, lora, embedding, etc.)
    /// </summary>
    public string? ModelType { get; set; }

    /// <summary>
    /// Rating (if applicable)
    /// </summary>
    public double? Rating { get; set; }

    /// <summary>
    /// Number of downloads
    /// </summary>
    public int? DownloadCount { get; set; }

    /// <summary>
    /// Associated tags
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Model versions
    /// </summary>
    public List<ModelVersionDto> Versions { get; set; } = new();

    /// <summary>
    /// Associated images
    /// </summary>
    public List<ModelImageDto> Images { get; set; } = new();

    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last updated timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Represents a version of a model
/// </summary>
public class ModelVersionDto
{
    /// <summary>
    /// Version identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Version name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Version description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Download URL
    /// </summary>
    public string? DownloadUrl { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long? FileSize { get; set; }

    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Represents an image associated with a model
/// </summary>
public class ModelImageDto
{
    /// <summary>
    /// Image identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Image URL
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Thumbnail URL
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// Image width
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Image height
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// Whether this is the primary image
    /// </summary>
    public bool IsPrimary { get; set; }
}
