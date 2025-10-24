namespace UMLMM.Core.Domain.Entities;

public class Image
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public string? Sha256 { get; set; }
    public string? Url { get; set; }
    public string? SampleUrl { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public long? FileSize { get; set; }
    public string? FileExtension { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public Post Post { get; set; } = null!;
}
