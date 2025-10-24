namespace UMLMM.Core.Entities;

/// <summary>
/// Represents an ingestion source (e.g., Ollama, CivitAI, etc.)
/// </summary>
public class Source
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
