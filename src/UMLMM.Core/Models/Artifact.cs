namespace UMLMM.Core.Models;

public class Artifact
{
    public int Id { get; set; }
    public required string SourceId { get; set; }
    public required string ExternalId { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Path { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
