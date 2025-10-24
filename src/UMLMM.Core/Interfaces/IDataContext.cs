using UMLMM.Core.Models;

namespace UMLMM.Core.Interfaces;

/// <summary>
/// Interface for data context operations
/// </summary>
public interface IDataContext
{
    /// <summary>
    /// Creates a new fetch run record
    /// </summary>
    Task<FetchRun> CreateFetchRunAsync(DataSource source, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing fetch run record
    /// </summary>
    Task UpdateFetchRunAsync(FetchRun fetchRun, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the most recent fetch run for a source
    /// </summary>
    Task<FetchRun?> GetLatestFetchRunAsync(DataSource source, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if there is a running fetch job for the specified source
    /// </summary>
    Task<bool> HasRunningFetchAsync(DataSource source, CancellationToken cancellationToken = default);
}
