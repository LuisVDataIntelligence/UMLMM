namespace UMLMM.Core.Entities;

/// <summary>
/// Represents a fetch/ingestion run
/// </summary>
public class FetchRun
{
    public int Id { get; set; }
    public int SourceId { get; set; }
    public Guid RunId { get; set; } // Unique run identifier for logging correlation
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = "running"; // running, completed, failed
    public int ModelsProcessed { get; set; }
    public int VersionsProcessed { get; set; }
    public int ArtifactsProcessed { get; set; }
    public string? ErrorMessage { get; set; }

    // Navigation properties
    public Source? Source { get; set; }
}
