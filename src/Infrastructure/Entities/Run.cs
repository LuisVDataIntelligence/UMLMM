namespace Infrastructure.Entities;

public class Run
{
    public int Id { get; set; }
    public string? WorkflowName { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ResultData { get; set; }
    public string? ErrorMessage { get; set; }
}
