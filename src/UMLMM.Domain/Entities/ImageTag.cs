namespace UMLMM.Domain.Entities;

public class ImageTag
{
    public Guid ImageId { get; set; }
    public Image Image { get; set; } = default!;
    
    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = default!;
    
    public DateTime CreatedAt { get; set; }
}
