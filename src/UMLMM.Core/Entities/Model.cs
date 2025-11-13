using System.Text.Json;

namespace UMLMM.Core.Entities;

/// <summary>
/// Represents an AI model from various sources
/// </summary>
public class Model
{
    public int Id { get; set; }
    public int SourceId { get; set; }
    public required string ExternalId { get; set; } // Unique ID from source (e.g., model name in Ollama)
    public required string Name { get; set; }
    public string? Description { get; set; }
    public JsonDocument? Metadata { get; set; } // JSONB column for flexible metadata
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Source? Source { get; set; }
    public ICollection<ModelVersion> Versions { get; set; } = new List<ModelVersion>();
}
