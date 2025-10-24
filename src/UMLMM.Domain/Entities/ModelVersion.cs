namespace UMLMM.Domain.Entities;

public class ModelVersion
{
    public int Id { get; set; }
    public int ModelId { get; set; }
    public required string ExternalId { get; set; }
    public string? VersionLabel { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? Raw { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Model Model { get; set; } = null!;
    public ICollection<Artifact> Artifacts { get; set; } = new List<Artifact>();
    public ICollection<Image> Images { get; set; } = new List<Image>();
}
