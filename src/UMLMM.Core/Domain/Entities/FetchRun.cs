namespace UMLMM.Core.Domain.Entities;

public class FetchRun
{
    public int Id { get; set; }
    public int SourceId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int PostsFetched { get; set; }
    public int PostsCreated { get; set; }
    public int PostsUpdated { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    
    public Source Source { get; set; } = null!;
}
