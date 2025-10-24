namespace Contracts.DTOs;

public class ModelDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int VersionCount { get; set; }
    public IEnumerable<string> Tags { get; set; } = new List<string>();
}

public class ModelDetailDto : ModelDto
{
    public IEnumerable<ModelVersionSummaryDto> Versions { get; set; } = new List<ModelVersionSummaryDto>();
}

public class ModelVersionSummaryDto
{
    public int Id { get; set; }
    public string VersionName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
