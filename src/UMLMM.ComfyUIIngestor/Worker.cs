using Microsoft.Extensions.Options;
using UMLMM.ComfyUIIngestor.Configuration;
using UMLMM.ComfyUIIngestor.Services;

namespace UMLMM.ComfyUIIngestor;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IWorkflowIngestService _ingestService;
    private readonly ComfyUIIngestorOptions _options;

    public Worker(
        ILogger<Worker> logger,
        IWorkflowIngestService ingestService,
        IOptions<ComfyUIIngestorOptions> options)
    {
        _logger = logger;
        _ingestService = ingestService;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ComfyUI Ingestor Worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Starting workflow ingestion cycle");
                var fetchRun = await _ingestService.IngestWorkflowsAsync(stoppingToken);
                _logger.LogInformation(
                    "Ingestion cycle completed. Created: {Created}, Updated: {Updated}, NoOp: {NoOp}, Errors: {Errors}",
                    fetchRun.CreatedCount, fetchRun.UpdatedCount, fetchRun.NoOpCount, fetchRun.ErrorCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during ingestion cycle");
            }

            var delay = TimeSpan.FromSeconds(_options.IntervalSeconds);
            _logger.LogInformation("Waiting {Delay} before next ingestion cycle", delay);
            await Task.Delay(delay, stoppingToken);
        }

        _logger.LogInformation("ComfyUI Ingestor Worker stopped");
    }
}
