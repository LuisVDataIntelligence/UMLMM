namespace Contracts.DTOs;

public class ImageDto
{
    public int Id { get; set; }
    public int? ModelVersionId { get; set; }
    public string? ModelId { get; set; }
    public string? Url { get; set; }
    public string? Hash { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? Rating { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
}
