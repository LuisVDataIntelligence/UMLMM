namespace Contracts.DTOs;

public class ModelVersionDto
{
    public int Id { get; set; }
    public int ModelId { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public string VersionName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DownloadUrl { get; set; }
    public long? FileSizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
