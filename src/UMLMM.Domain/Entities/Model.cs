namespace UMLMM.Domain.Entities;

public class Model
{
    public int Id { get; set; }
    public int SourceId { get; set; }
    public required string ExternalId { get; set; }
    public required string Name { get; set; }
    public string? Type { get; set; }
    public string? Description { get; set; }
    public bool Nsfw { get; set; }
    public string? Raw { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Source Source { get; set; } = null!;
    public ICollection<ModelVersion> Versions { get; set; } = new List<ModelVersion>();
    public ICollection<ModelTag> ModelTags { get; set; } = new List<ModelTag>();
}
