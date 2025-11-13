using Contracts.DTOs;

namespace BlazorFrontend.Services;

/// <summary>
/// Interface for API Gateway client
/// </summary>
public interface IApiClient
{
    /// <summary>
    /// Search models with pagination and filters
    /// </summary>
    Task<PagedResultDto<ModelDto>> SearchModelsAsync(SearchRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific model by ID
    /// </summary>
    Task<ModelDto?> GetModelAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a model
    /// </summary>
    Task<ModelDto> UpdateModelAsync(int id, ModelDto model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get recent runs
    /// </summary>
    Task<PagedResultDto<RunDto>> GetRunsAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific run by ID
    /// </summary>
    Task<RunDto?> GetRunAsync(int id, CancellationToken cancellationToken = default);
}
