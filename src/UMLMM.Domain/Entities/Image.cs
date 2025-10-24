namespace UMLMM.Domain.Entities;

public class Image
{
    public Guid Id { get; set; }
    public string SourceId { get; set; } = default!;
    public string ExternalId { get; set; } = default!;
    public string? Sha256 { get; set; }
    public string? PreviewUrl { get; set; }
    public string? OriginalUrl { get; set; }
    public string? Rating { get; set; }
    public string? Metadata { get; set; } // JSONB in PostgreSQL
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public ICollection<ImageTag> ImageTags { get; set; } = new List<ImageTag>();
}
