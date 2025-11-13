using UMLMM.Domain.Common;

namespace UMLMM.Domain.Entities;

public class Prompt : BaseEntity
{
    public int PromptId { get; set; }
    public int? ModelId { get; set; }
    public int? ModelVersionId { get; set; }
    public int SourceId { get; set; }
    public required string Text { get; set; }
    public string? Metadata { get; set; }

    public Model? Model { get; set; }
    public ModelVersion? ModelVersion { get; set; }
    public Source Source { get; set; } = null!;
}
