using UMLMM.Domain.Common;

namespace UMLMM.Domain.Entities;

public class ModelVersion : BaseEntity
{
    public int ModelVersionId { get; set; }
    public int ModelId { get; set; }
    public required string VersionLabel { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? Checksum { get; set; }
    public string? Metadata { get; set; }

    public Model Model { get; set; } = null!;
    public ICollection<Artifact> Artifacts { get; set; } = new List<Artifact>();
    public ICollection<Image> Images { get; set; } = new List<Image>();
    public ICollection<Prompt> Prompts { get; set; } = new List<Prompt>();
}
