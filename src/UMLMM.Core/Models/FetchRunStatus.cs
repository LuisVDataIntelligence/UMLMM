namespace UMLMM.Core.Models;

/// <summary>
/// Status of a fetch run
/// </summary>
public enum FetchRunStatus
{
    Queued,
    Running,
    Completed,
    Failed,
    Cancelled
}
