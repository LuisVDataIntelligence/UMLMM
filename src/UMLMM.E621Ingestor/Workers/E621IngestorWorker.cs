using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UMLMM.E621Ingestor.Services;

namespace UMLMM.E621Ingestor.Workers;

public class E621IngestorWorker : BackgroundService
{
    private readonly IE621IngestorService _ingestorService;
    private readonly ILogger<E621IngestorWorker> _logger;

    public E621IngestorWorker(
        IE621IngestorService ingestorService,
        ILogger<E621IngestorWorker> logger)
    {
        _ingestorService = ingestorService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("E621 Ingestor Worker starting");

        try
        {
            await _ingestorService.IngestAsync(stoppingToken);
            _logger.LogInformation("E621 Ingestor Worker completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "E621 Ingestor Worker failed");
            throw;
        }
    }
}
