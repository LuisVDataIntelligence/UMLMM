namespace UMLMM.Domain.Entities;

public class ModelTag
{
    public int ModelId { get; set; }
    public int TagId { get; set; }

    public Model Model { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
