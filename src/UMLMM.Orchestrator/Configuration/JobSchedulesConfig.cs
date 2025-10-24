namespace UMLMM.Orchestrator.Configuration;

/// <summary>
/// Configuration for all job schedules
/// </summary>
public class JobSchedulesConfig
{
    public JobScheduleConfig CivitAI { get; set; } = new();
    public JobScheduleConfig Danbooru { get; set; } = new();
    public JobScheduleConfig E621 { get; set; } = new();
    public JobScheduleConfig ComfyUI { get; set; } = new();
    public JobScheduleConfig Ollama { get; set; } = new();
}
