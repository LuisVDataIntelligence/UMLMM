namespace Infrastructure.Entities;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public ICollection<ModelTag> ModelTags { get; set; } = new List<ModelTag>();
}
