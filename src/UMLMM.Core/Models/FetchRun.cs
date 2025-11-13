namespace UMLMM.Core.Models;

/// <summary>
/// Represents a single fetch run for a data source
/// </summary>
public class FetchRun
{
    public int Id { get; set; }
    public DataSource Source { get; set; }
    public FetchRunStatus Status { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int RecordsFetched { get; set; }
    public int RecordsProcessed { get; set; }
    public int RecordsFailed { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorDetails { get; set; }

    /// <summary>
    /// Duration of the run in milliseconds
    /// </summary>
    public long? DurationMs => EndTime.HasValue 
        ? (long)(EndTime.Value - StartTime).TotalMilliseconds 
        : null;
}
