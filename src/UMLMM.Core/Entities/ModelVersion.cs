using System.Text.Json;

namespace UMLMM.Core.Entities;

/// <summary>
/// Represents a specific version/tag of a model
/// </summary>
public class ModelVersion
{
    public int Id { get; set; }
    public int ModelId { get; set; }
    public required string Tag { get; set; } // Version tag (e.g., "latest", "7b", etc.)
    public required string ExternalId { get; set; } // Unique ID from source (e.g., full model name with tag)
    public string? ParentModel { get; set; } // Parent/base model if any
    public JsonDocument? Parameters { get; set; } // Model parameters (temperature, context_length, etc.)
    public JsonDocument? Metadata { get; set; } // Additional metadata
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Model? Model { get; set; }
    public ICollection<ModelArtifact> Artifacts { get; set; } = new List<ModelArtifact>();
}
