using UMLMM.Core.Domain.Enums;

namespace UMLMM.Core.Domain.Entities;

public class Post
{
    public int Id { get; set; }
    public int SourceId { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Rating Rating { get; set; }
    public DateTime? ExternalCreatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public Source Source { get; set; } = null!;
    public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    public ICollection<Image> Images { get; set; } = new List<Image>();
}
