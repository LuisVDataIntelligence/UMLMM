namespace UMLMM.Domain.Common;

public abstract class BaseEntity
{
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
