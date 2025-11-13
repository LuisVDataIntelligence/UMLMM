using UMLMM.Domain.Common;
using UMLMM.Domain.Enums;

namespace UMLMM.Domain.Entities;

public class Image : BaseEntity
{
    public int ImageId { get; set; }
    public int? ModelId { get; set; }
    public int? ModelVersionId { get; set; }
    public required string Url { get; set; }
    public NsfwRating Rating { get; set; } = NsfwRating.Unknown;
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? Sha256 { get; set; }
    public string? Metadata { get; set; }

    public Model? Model { get; set; }
    public ModelVersion? ModelVersion { get; set; }
}
