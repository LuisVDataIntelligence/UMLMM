using System.Collections.Concurrent;
using UMLMM.Core.Interfaces;
using UMLMM.Core.Models;

namespace UMLMM.Core.Services;

/// <summary>
/// In-memory implementation of IDataContext for development/testing
/// </summary>
public class InMemoryDataContext : IDataContext
{
    private readonly ConcurrentDictionary<int, FetchRun> _fetchRuns = new();
    private int _nextId = 1;

    public Task<FetchRun> CreateFetchRunAsync(DataSource source, CancellationToken cancellationToken = default)
    {
        var fetchRun = new FetchRun
        {
            Id = Interlocked.Increment(ref _nextId),
            Source = source,
            Status = FetchRunStatus.Queued,
            StartTime = DateTime.UtcNow,
            RecordsFetched = 0,
            RecordsProcessed = 0,
            RecordsFailed = 0
        };

        _fetchRuns.TryAdd(fetchRun.Id, fetchRun);
        return Task.FromResult(fetchRun);
    }

    public Task UpdateFetchRunAsync(FetchRun fetchRun, CancellationToken cancellationToken = default)
    {
        _fetchRuns[fetchRun.Id] = fetchRun;
        return Task.CompletedTask;
    }

    public Task<FetchRun?> GetLatestFetchRunAsync(DataSource source, CancellationToken cancellationToken = default)
    {
        var latest = _fetchRuns.Values
            .Where(fr => fr.Source == source)
            .OrderByDescending(fr => fr.StartTime)
            .FirstOrDefault();

        return Task.FromResult(latest);
    }

    public Task<bool> HasRunningFetchAsync(DataSource source, CancellationToken cancellationToken = default)
    {
        var hasRunning = _fetchRuns.Values
            .Any(fr => fr.Source == source && 
                      (fr.Status == FetchRunStatus.Queued || fr.Status == FetchRunStatus.Running));

        return Task.FromResult(hasRunning);
    }
}
