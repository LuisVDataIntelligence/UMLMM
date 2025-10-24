using Quartz;
using UMLMM.Core.Interfaces;
using UMLMM.Core.Models;

namespace UMLMM.Orchestrator.Jobs;

/// <summary>
/// Base class for all data ingestion jobs with no-overlap logic
/// </summary>
[DisallowConcurrentExecution]
public abstract class BaseIngestionJob : IJob
{
    private readonly IDataContext _dataContext;
    private readonly ILogger _logger;

    protected abstract DataSource Source { get; }

    protected BaseIngestionJob(IDataContext dataContext, ILogger logger)
    {
        _dataContext = dataContext;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var cancellationToken = context.CancellationToken;

        // Check if there's already a running job for this source
        if (await _dataContext.HasRunningFetchAsync(Source, cancellationToken))
        {
            _logger.LogWarning("Job for {Source} is already running, skipping this execution", Source);
            return;
        }

        // Create a new fetch run
        var fetchRun = await _dataContext.CreateFetchRunAsync(Source, cancellationToken);
        _logger.LogInformation("Starting fetch run {RunId} for {Source}", fetchRun.Id, Source);

        try
        {
            // Update status to running
            fetchRun.Status = FetchRunStatus.Running;
            await _dataContext.UpdateFetchRunAsync(fetchRun, cancellationToken);

            // Execute the actual ingestion logic
            await ExecuteIngestionAsync(fetchRun, context);

            // Mark as completed
            fetchRun.Status = FetchRunStatus.Completed;
            fetchRun.EndTime = DateTime.UtcNow;
            await _dataContext.UpdateFetchRunAsync(fetchRun, cancellationToken);

            _logger.LogInformation(
                "Completed fetch run {RunId} for {Source}. Duration: {Duration}ms, Fetched: {Fetched}, Processed: {Processed}, Failed: {Failed}",
                fetchRun.Id, Source, fetchRun.DurationMs, fetchRun.RecordsFetched, fetchRun.RecordsProcessed, fetchRun.RecordsFailed);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            fetchRun.Status = FetchRunStatus.Cancelled;
            fetchRun.EndTime = DateTime.UtcNow;
            fetchRun.ErrorMessage = "Job was cancelled";
            await _dataContext.UpdateFetchRunAsync(fetchRun, cancellationToken: default);

            _logger.LogWarning("Fetch run {RunId} for {Source} was cancelled", fetchRun.Id, Source);
        }
        catch (Exception ex)
        {
            fetchRun.Status = FetchRunStatus.Failed;
            fetchRun.EndTime = DateTime.UtcNow;
            fetchRun.ErrorMessage = ex.Message;
            fetchRun.ErrorDetails = ex.ToString();
            await _dataContext.UpdateFetchRunAsync(fetchRun, cancellationToken: default);

            _logger.LogError(ex, "Fetch run {RunId} for {Source} failed", fetchRun.Id, Source);
        }
    }

    /// <summary>
    /// Implement this method to perform the actual data ingestion
    /// </summary>
    protected abstract Task ExecuteIngestionAsync(FetchRun fetchRun, IJobExecutionContext context);
}
