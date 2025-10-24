namespace Contracts.DTOs;

public class TagDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int ModelCount { get; set; }
}

public class AssignTagRequest
{
    public int ModelId { get; set; }
    public int TagId { get; set; }
}

public class RemoveTagRequest
{
    public int ModelId { get; set; }
    public int TagId { get; set; }
}
