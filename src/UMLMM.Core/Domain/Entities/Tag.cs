namespace UMLMM.Core.Domain.Entities;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
}
