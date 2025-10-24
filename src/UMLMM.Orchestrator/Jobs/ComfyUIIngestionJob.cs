using Quartz;
using UMLMM.Core.Interfaces;
using UMLMM.Core.Models;

namespace UMLMM.Orchestrator.Jobs;

/// <summary>
/// Job for ingesting data from ComfyUI
/// </summary>
public class ComfyUIIngestionJob : BaseIngestionJob
{
    private readonly ILogger<ComfyUIIngestionJob> _logger;

    protected override DataSource Source => DataSource.ComfyUI;

    public ComfyUIIngestionJob(IDataContext dataContext, ILogger<ComfyUIIngestionJob> logger) 
        : base(dataContext, logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteIngestionAsync(FetchRun fetchRun, IJobExecutionContext context)
    {
        _logger.LogInformation("Executing ComfyUI ingestion...");

        // Simulate ingestion work
        await Task.Delay(TimeSpan.FromSeconds(5), context.CancellationToken);

        // Update statistics
        fetchRun.RecordsFetched = 80;
        fetchRun.RecordsProcessed = 78;
        fetchRun.RecordsFailed = 2;

        _logger.LogInformation("ComfyUI ingestion completed");
    }
}
