namespace UMLMM.DanbooruIngestor.Configuration;

public class DanbooruSettings
{
    public const string SectionName = "Danbooru";
    
    public string BaseUrl { get; set; } = "https://danbooru.donmai.us";
    public string? ApiKey { get; set; }
    public string? Username { get; set; }
    public int PageSize { get; set; } = 100;
    public int MaxPages { get; set; } = 10;
    public string? Tags { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
    public int CircuitBreakerThreshold { get; set; } = 5;
    public int CircuitBreakerDurationSeconds { get; set; } = 60;
}
