namespace Infrastructure.Entities;

public class ModelTag
{
    public int ModelId { get; set; }
    public int TagId { get; set; }
    public DateTime AssignedAt { get; set; }
    
    // Navigation properties
    public Model Model { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
