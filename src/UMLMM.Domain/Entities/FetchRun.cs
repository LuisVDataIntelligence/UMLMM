namespace UMLMM.Domain.Entities;

public class FetchRun
{
    public int Id { get; set; }
    public int SourceId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = "running";
    public int RecordsCreated { get; set; }
    public int RecordsUpdated { get; set; }
    public int RecordsNoOp { get; set; }
    public int RecordsError { get; set; }
    public string? Cursor { get; set; }
    public string? ErrorMessage { get; set; }

    public Source Source { get; set; } = null!;
}
