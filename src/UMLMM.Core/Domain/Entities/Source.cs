namespace UMLMM.Core.Domain.Entities;

public class Source
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<FetchRun> FetchRuns { get; set; } = new List<FetchRun>();
}
