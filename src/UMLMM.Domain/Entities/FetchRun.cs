namespace UMLMM.Domain.Entities;

public class FetchRun
{
    public Guid Id { get; set; }
    public string RunId { get; set; } = default!;
    public string SourceId { get; set; } = default!;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int CreatedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int NoOpCount { get; set; }
    public int ErrorCount { get; set; }
    public string? Parameters { get; set; } // JSONB for query params
    public string? ErrorDetails { get; set; }
}
