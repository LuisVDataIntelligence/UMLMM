using Quartz;
using UMLMM.Core.Interfaces;
using UMLMM.Core.Models;

namespace UMLMM.Orchestrator.Jobs;

/// <summary>
/// Job for ingesting data from Danbooru
/// </summary>
public class DanbooruIngestionJob : BaseIngestionJob
{
    private readonly ILogger<DanbooruIngestionJob> _logger;

    protected override DataSource Source => DataSource.Danbooru;

    public DanbooruIngestionJob(IDataContext dataContext, ILogger<DanbooruIngestionJob> logger) 
        : base(dataContext, logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteIngestionAsync(FetchRun fetchRun, IJobExecutionContext context)
    {
        _logger.LogInformation("Executing Danbooru ingestion...");

        // Simulate ingestion work
        await Task.Delay(TimeSpan.FromSeconds(5), context.CancellationToken);

        // Update statistics
        fetchRun.RecordsFetched = 150;
        fetchRun.RecordsProcessed = 145;
        fetchRun.RecordsFailed = 5;

        _logger.LogInformation("Danbooru ingestion completed");
    }
}
