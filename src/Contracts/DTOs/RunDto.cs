namespace Contracts.DTOs;

/// <summary>
/// Represents an ingestion run
/// </summary>
public class RunDto
{
    /// <summary>
    /// Run identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Source system for this run
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Run status
    /// </summary>
    public RunStatus Status { get; set; }

    /// <summary>
    /// Start timestamp
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// End timestamp (if completed)
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Number of records processed
    /// </summary>
    public int RecordsProcessed { get; set; }

    /// <summary>
    /// Number of records created
    /// </summary>
    public int RecordsCreated { get; set; }

    /// <summary>
    /// Number of records updated
    /// </summary>
    public int RecordsUpdated { get; set; }

    /// <summary>
    /// Number of errors encountered
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Error message (if failed)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Duration in seconds
    /// </summary>
    public double? DurationSeconds => CompletedAt.HasValue 
        ? (CompletedAt.Value - StartedAt).TotalSeconds 
        : null;
}

/// <summary>
/// Run status enumeration
/// </summary>
public enum RunStatus
{
    /// <summary>
    /// Run is queued but not started
    /// </summary>
    Queued,

    /// <summary>
    /// Run is currently running
    /// </summary>
    Running,

    /// <summary>
    /// Run completed successfully
    /// </summary>
    Completed,

    /// <summary>
    /// Run failed with errors
    /// </summary>
    Failed,

    /// <summary>
    /// Run was cancelled
    /// </summary>
    Cancelled
}
