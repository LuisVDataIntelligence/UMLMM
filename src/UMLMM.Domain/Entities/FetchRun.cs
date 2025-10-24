using UMLMM.Domain.Common;
using UMLMM.Domain.Enums;

namespace UMLMM.Domain.Entities;

public class FetchRun : BaseEntity
{
    public int RunId { get; set; }
    public int SourceId { get; set; }
    public FetchStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string? Stats { get; set; }
    public string? ErrorText { get; set; }

    public Source Source { get; set; } = null!;
}
