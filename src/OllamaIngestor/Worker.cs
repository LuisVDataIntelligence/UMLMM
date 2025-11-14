using UMLMM.Infrastructure.Repositories;
using OllamaIngestor.Services;

namespace OllamaIngestor;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public Worker(
        ILogger<Worker> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Get the interval from configuration (default to 1 hour)
        var intervalMinutes = _configuration.GetValue<int>("Ollama:IntervalMinutes", 60);
        var runOnStartup = _configuration.GetValue<bool>("Ollama:RunOnStartup", true);

        _logger.LogInformation("OllamaIngestor started. Interval: {Interval} minutes, RunOnStartup: {RunOnStartup}", 
            intervalMinutes, runOnStartup);

        if (runOnStartup)
        {
            await PerformIngestionAsync(stoppingToken);
        }

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(intervalMinutes));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
                await PerformIngestionAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("OllamaIngestor is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in OllamaIngestor main loop");
            }
        }
    }

    private async Task PerformIngestionAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var modelRepository = scope.ServiceProvider.GetRequiredService<IModelRepository>();
        var ingestionService = scope.ServiceProvider.GetRequiredService<OllamaIngestionService>();

        var runId = Guid.NewGuid();
        
        try
        {
            _logger.LogInformation("Starting Ollama ingestion run {RunId}", runId);

            // Ensure Ollama source exists
            var source = await modelRepository.UpsertSourceAsync("Ollama", "Local Ollama models", cancellationToken);

            // Create fetch run
            var fetchRun = await modelRepository.CreateFetchRunAsync(source.Id, runId, cancellationToken);

            // Perform ingestion
            var (models, versions, artifacts) = await ingestionService.IngestModelsAsync(source.Id, runId, cancellationToken);

            // Update fetch run with success
            await modelRepository.UpdateFetchRunAsync(
                fetchRun.Id,
                "completed",
                models,
                versions,
                artifacts,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Completed Ollama ingestion run {RunId}. Models: {Models}, Versions: {Versions}, Artifacts: {Artifacts}",
                runId, models, versions, artifacts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed Ollama ingestion run {RunId}", runId);
            
            // Try to update fetch run with failure
            try
            {
                var source = await modelRepository.UpsertSourceAsync("Ollama", "Local Ollama models", cancellationToken);
                var fetchRuns = await modelRepository.CreateFetchRunAsync(source.Id, runId, cancellationToken);
                await modelRepository.UpdateFetchRunAsync(
                    fetchRuns.Id,
                    "failed",
                    0, 0, 0,
                    ex.Message,
                    cancellationToken);
            }
            catch (Exception updateEx)
            {
                _logger.LogError(updateEx, "Failed to update fetch run status");
            }
        }
    }
}
