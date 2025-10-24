using UMLMM.Domain.Common;

namespace UMLMM.Domain.Entities;

public class Source : BaseEntity
{
    public int SourceId { get; set; }
    public required string Name { get; set; }
    public required string BaseUrl { get; set; }

    public ICollection<Model> Models { get; set; } = new List<Model>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public ICollection<Workflow> Workflows { get; set; } = new List<Workflow>();
    public ICollection<Prompt> Prompts { get; set; } = new List<Prompt>();
    public ICollection<FetchRun> FetchRuns { get; set; } = new List<FetchRun>();
}
