using UMLMM.Domain.Common;

namespace UMLMM.Domain.Entities;

public class Workflow : BaseEntity
{
    public int WorkflowId { get; set; }
    public int SourceId { get; set; }
    public required string ExternalId { get; set; }
    public required string Title { get; set; }
    public string? Graph { get; set; }
    public int? NodesCount { get; set; }

    public Source Source { get; set; } = null!;
}
