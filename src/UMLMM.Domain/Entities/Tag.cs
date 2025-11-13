using UMLMM.Domain.Common;

namespace UMLMM.Domain.Entities;

public class Tag : BaseEntity
{
    public int TagId { get; set; }
    public required string Name { get; set; }
    public required string NormalizedName { get; set; }
    public int? SourceId { get; set; }

    public Source? Source { get; set; }
    public ICollection<ModelTag> ModelTags { get; set; } = new List<ModelTag>();
}
