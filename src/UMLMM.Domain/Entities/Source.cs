namespace UMLMM.Domain.Entities;

public class Source
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
    public string? BaseUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Model> Models { get; set; } = new List<Model>();
    public ICollection<FetchRun> FetchRuns { get; set; } = new List<FetchRun>();
}
