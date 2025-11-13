namespace Contracts.DTOs;

/// <summary>
/// Request parameters for model search
/// </summary>
public class SearchRequestDto
{
    /// <summary>
    /// Text search query
    /// </summary>
    public string? Query { get; set; }

    /// <summary>
    /// Filter by source
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Filter by minimum rating
    /// </summary>
    public double? MinRating { get; set; }

    /// <summary>
    /// Filter by tags
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Sort field
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort direction (asc/desc)
    /// </summary>
    public string? SortDirection { get; set; }
}
