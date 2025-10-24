namespace Infrastructure.Entities;

public class ModelVersion
{
    public int Id { get; set; }
    public int ModelId { get; set; }
    public string VersionName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DownloadUrl { get; set; }
    public long? FileSizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public Model Model { get; set; } = null!;
    public ICollection<Image> Images { get; set; } = new List<Image>();
}
