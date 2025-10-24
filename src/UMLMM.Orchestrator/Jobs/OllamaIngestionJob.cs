using Quartz;
using UMLMM.Core.Interfaces;
using UMLMM.Core.Models;

namespace UMLMM.Orchestrator.Jobs;

/// <summary>
/// Job for ingesting data from Ollama
/// </summary>
public class OllamaIngestionJob : BaseIngestionJob
{
    private readonly ILogger<OllamaIngestionJob> _logger;

    protected override DataSource Source => DataSource.Ollama;

    public OllamaIngestionJob(IDataContext dataContext, ILogger<OllamaIngestionJob> logger) 
        : base(dataContext, logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteIngestionAsync(FetchRun fetchRun, IJobExecutionContext context)
    {
        _logger.LogInformation("Executing Ollama ingestion...");

        // Simulate ingestion work
        await Task.Delay(TimeSpan.FromSeconds(5), context.CancellationToken);

        // Update statistics
        fetchRun.RecordsFetched = 50;
        fetchRun.RecordsProcessed = 49;
        fetchRun.RecordsFailed = 1;

        _logger.LogInformation("Ollama ingestion completed");
    }
}
