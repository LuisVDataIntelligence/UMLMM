namespace OllamaIngestor.Models;

public class OllamaModel
{
    public required string Name { get; set; }
    public string? ModifiedAt { get; set; }
    public long? Size { get; set; }
    public string? Digest { get; set; }
    public Dictionary<string, object>? Details { get; set; }
}

public class OllamaModelShow
{
    public required string Modelfile { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
    public string? Template { get; set; }
    public Dictionary<string, object>? Details { get; set; }
}

public class OllamaListResponse
{
    public List<OllamaModel> Models { get; set; } = new();
}
