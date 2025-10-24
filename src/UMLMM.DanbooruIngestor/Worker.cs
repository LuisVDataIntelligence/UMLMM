using UMLMM.DanbooruIngestor.Services;

namespace UMLMM.DanbooruIngestor;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Danbooru Ingestor Worker starting");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var ingestionService = scope.ServiceProvider.GetRequiredService<DanbooruIngestionService>();
            
            var runId = $"danbooru-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
            var fetchRun = await ingestionService.IngestPostsAsync(runId, stoppingToken);
            
            _logger.LogInformation(
                "Ingestion completed: {RunId}, Created={Created}, Updated={Updated}, NoOp={NoOp}, Errors={Errors}",
                fetchRun.RunId,
                fetchRun.CreatedCount,
                fetchRun.UpdatedCount,
                fetchRun.NoOpCount,
                fetchRun.ErrorCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running ingestion worker");
            throw;
        }
    }
}
