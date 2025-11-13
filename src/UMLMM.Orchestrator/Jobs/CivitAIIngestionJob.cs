using Quartz;
using UMLMM.Core.Interfaces;
using UMLMM.Core.Models;

namespace UMLMM.Orchestrator.Jobs;

/// <summary>
/// Job for ingesting data from CivitAI
/// </summary>
public class CivitAIIngestionJob : BaseIngestionJob
{
    private readonly ILogger<CivitAIIngestionJob> _logger;

    protected override DataSource Source => DataSource.CivitAI;

    public CivitAIIngestionJob(IDataContext dataContext, ILogger<CivitAIIngestionJob> logger) 
        : base(dataContext, logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteIngestionAsync(FetchRun fetchRun, IJobExecutionContext context)
    {
        _logger.LogInformation("Executing CivitAI ingestion...");

        // Simulate ingestion work
        await Task.Delay(TimeSpan.FromSeconds(5), context.CancellationToken);

        // Update statistics
        fetchRun.RecordsFetched = 100;
        fetchRun.RecordsProcessed = 95;
        fetchRun.RecordsFailed = 5;

        _logger.LogInformation("CivitAI ingestion completed");
    }
}
