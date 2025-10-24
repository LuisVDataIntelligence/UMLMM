namespace UMLMM.ComfyUIIngestor.Configuration;

public class ComfyUIIngestorOptions
{
    public const string SectionName = "ComfyUIIngestor";

    public List<string> BaseDirectories { get; set; } = new();
    public List<string> IncludePatterns { get; set; } = new() { "*.json" };
    public List<string> ExcludePatterns { get; set; } = new();
    public int IntervalSeconds { get; set; } = 3600;
    public string SourceId { get; set; } = "comfyui";
}
