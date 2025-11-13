using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using UMLMM.Infrastructure.Data;
using UMLMM.Ingestors.CivitAI.CivitAI.Client;
using UMLMM.Ingestors.CivitAI.Services;

namespace UMLMM.Ingestors.CivitAI;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                "logs/civitai-ingestor-.log",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        try
        {
            Log.Information("Starting CivitAI Ingestor");

            var host = CreateHostBuilder(args).Build();

            // Run database migrations
            using (var scope = host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<UmlmmDbContext>();
                Log.Information("Applying database migrations...");
                await dbContext.Database.MigrateAsync();
                Log.Information("Database migrations applied successfully");
            }

            // Run ingestion
            using (var scope = host.Services.CreateScope())
            {
                var ingestionService = scope.ServiceProvider.GetRequiredService<CivitAIIngestionService>();
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                var startPage = configuration.GetValue<int?>("CivitAI:StartPage") ?? 1;
                var pageSize = configuration.GetValue<int?>("CivitAI:PageSize") ?? 100;
                var maxPages = configuration.GetValue<int?>("CivitAI:MaxPages");
                var apiKey = configuration.GetValue<string?>("CivitAI:ApiKey");

                Log.Information("Starting ingestion with startPage={StartPage}, pageSize={PageSize}, maxPages={MaxPages}",
                    startPage, pageSize, maxPages);

                var fetchRun = await ingestionService.IngestAsync(
                    startPage: startPage,
                    pageSize: pageSize,
                    maxPages: maxPages,
                    apiKey: apiKey);

                Log.Information("Ingestion completed: Status={Status}, Created={Created}, Updated={Updated}, NoOp={NoOp}, Errors={Errors}",
                    fetchRun.Status, fetchRun.RecordsCreated, fetchRun.RecordsUpdated, fetchRun.RecordsNoOp, fetchRun.RecordsError);

                return fetchRun.Status == "completed" ? 0 : 1;
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true);
                config.AddEnvironmentVariables(prefix: "UMLMM_");
                config.AddCommandLine(args);
            })
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;

                // Configure DbContext
                services.AddDbContext<UmlmmDbContext>(options =>
                {
                    var connectionString = configuration.GetConnectionString("DefaultConnection")
                        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");
                    options.UseNpgsql(connectionString);
                });

                // Configure HttpClient for CivitAI API
                services.AddHttpClient<CivitAIApiClient>(client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(60);
                    client.DefaultRequestHeaders.Add("User-Agent", "UMLMM-CivitAI-Ingestor/1.0");
                });

                // Register services
                services.AddScoped<CivitAIIngestionService>();
            });
}
