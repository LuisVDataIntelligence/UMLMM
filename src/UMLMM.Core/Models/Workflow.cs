namespace UMLMM.Core.Models;

public class Workflow
{
    public int Id { get; set; }
    public required string SourceId { get; set; }
    public required string ExternalId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? GraphJsonb { get; set; }
    public int NodesCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
