namespace UMLMM.Domain.Entities;

public class Image
{
    public int Id { get; set; }
    public int VersionId { get; set; }
    public required string ExternalId { get; set; }
    public string? PreviewUrl { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? Rating { get; set; }
    public string? Sha256 { get; set; }
    public string? Raw { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ModelVersion Version { get; set; } = null!;
}
