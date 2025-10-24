using UMLMM.Domain.Common;
using UMLMM.Domain.Enums;

namespace UMLMM.Domain.Entities;

public class Model : BaseEntity
{
    public int ModelId { get; set; }
    public int SourceId { get; set; }
    public required string ExternalId { get; set; }
    public required string Name { get; set; }
    public ModelType Type { get; set; }
    public string? Description { get; set; }
    public NsfwRating NsfwRating { get; set; } = NsfwRating.Unknown;
    public DateTime? UpdatedAt { get; set; }
    public string? Raw { get; set; }

    public Source Source { get; set; } = null!;
    public ICollection<ModelVersion> ModelVersions { get; set; } = new List<ModelVersion>();
    public ICollection<ModelTag> ModelTags { get; set; } = new List<ModelTag>();
    public ICollection<Image> Images { get; set; } = new List<Image>();
    public ICollection<Prompt> Prompts { get; set; } = new List<Prompt>();
}
