namespace UMLMM.Domain.Entities;

public class Artifact
{
    public int Id { get; set; }
    public int VersionId { get; set; }
    public required string ExternalId { get; set; }
    public string? FileKind { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? Sha256 { get; set; }
    public string? DownloadUrl { get; set; }
    public string? Raw { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ModelVersion Version { get; set; } = null!;
}
