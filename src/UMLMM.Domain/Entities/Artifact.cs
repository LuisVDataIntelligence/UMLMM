using UMLMM.Domain.Common;
using UMLMM.Domain.Enums;

namespace UMLMM.Domain.Entities;

public class Artifact : BaseEntity
{
    public int ArtifactId { get; set; }
    public int ModelVersionId { get; set; }
    public ArtifactKind Kind { get; set; }
    public required string FileName { get; set; }
    public long SizeBytes { get; set; }
    public string? Sha256 { get; set; }
    public required string Url { get; set; }
    public string? Metadata { get; set; }

    public ModelVersion ModelVersion { get; set; } = null!;
}
