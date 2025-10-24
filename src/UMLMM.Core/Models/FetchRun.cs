namespace UMLMM.Core.Models;

public class FetchRun
{
    public int Id { get; set; }
    public required string SourceId { get; set; }
    public required string RunId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int CreatedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int NoOpCount { get; set; }
    public int ErrorCount { get; set; }
    public string? ErrorDetails { get; set; }
}
