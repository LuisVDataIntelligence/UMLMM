using System.Text.Json;

namespace UMLMM.Core.Entities;

/// <summary>
/// Represents an artifact (file/layer) associated with a model version
/// </summary>
public class ModelArtifact
{
    public int Id { get; set; }
    public int ModelVersionId { get; set; }
    public required string Type { get; set; } // e.g., "layer", "modelfile", "config"
    public string? Digest { get; set; } // SHA256 or other hash
    public long? Size { get; set; } // Size in bytes
    public string? MediaType { get; set; }
    public JsonDocument? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ModelVersion? ModelVersion { get; set; }
}
