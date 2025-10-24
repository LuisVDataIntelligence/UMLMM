namespace UMLMM.Domain.Entities;

public class Tag
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ModelTag> ModelTags { get; set; } = new List<ModelTag>();
}
