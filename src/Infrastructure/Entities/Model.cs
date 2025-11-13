namespace Infrastructure.Entities;

public class Model
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<ModelVersion> Versions { get; set; } = new List<ModelVersion>();
    public ICollection<ModelTag> ModelTags { get; set; } = new List<ModelTag>();
}
