namespace UMLMM.Orchestrator.Configuration;

/// <summary>
/// Configuration for a single job schedule
/// </summary>
public class JobScheduleConfig
{
    public string CronSchedule { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
