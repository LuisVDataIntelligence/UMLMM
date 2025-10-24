using Quartz;
using UMLMM.Core.Interfaces;
using UMLMM.Core.Models;

namespace UMLMM.Orchestrator.Jobs;

/// <summary>
/// Job for ingesting data from e621
/// </summary>
public class E621IngestionJob : BaseIngestionJob
{
    private readonly ILogger<E621IngestionJob> _logger;

    protected override DataSource Source => DataSource.E621;

    public E621IngestionJob(IDataContext dataContext, ILogger<E621IngestionJob> logger) 
        : base(dataContext, logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteIngestionAsync(FetchRun fetchRun, IJobExecutionContext context)
    {
        _logger.LogInformation("Executing e621 ingestion...");

        // Simulate ingestion work
        await Task.Delay(TimeSpan.FromSeconds(5), context.CancellationToken);

        // Update statistics
        fetchRun.RecordsFetched = 120;
        fetchRun.RecordsProcessed = 118;
        fetchRun.RecordsFailed = 2;

        _logger.LogInformation("e621 ingestion completed");
    }
}
