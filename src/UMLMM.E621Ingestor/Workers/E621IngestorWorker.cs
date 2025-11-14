using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UMLMM.E621Ingestor.Services;

namespace UMLMM.E621Ingestor.Workers;

public class E621IngestorWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<E621IngestorWorker> _logger;

    public E621IngestorWorker(
        IServiceProvider serviceProvider,
        ILogger<E621IngestorWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("E621 Ingestor Worker starting");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var ingestorService = scope.ServiceProvider.GetRequiredService<IE621IngestorService>();
            await ingestorService.IngestAsync(stoppingToken);
            _logger.LogInformation("E621 Ingestor Worker completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "E621 Ingestor Worker failed");
            throw;
        }
    }
}
