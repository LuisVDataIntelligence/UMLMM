using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Serilog;
using UMLMM.DanbooruIngestor;
using UMLMM.DanbooruIngestor.Configuration;
using UMLMM.DanbooruIngestor.Danbooru;
using UMLMM.DanbooruIngestor.Mapping;
using UMLMM.DanbooruIngestor.Services;
using UMLMM.Infrastructure.Data;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    // Add Serilog
    builder.Services.AddSerilog();

    // Configure settings
    var danbooruSettings = builder.Configuration
        .GetSection(DanbooruSettings.SectionName)
        .Get<DanbooruSettings>() ?? new DanbooruSettings();
    builder.Services.AddSingleton(danbooruSettings);

    // Configure database
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        // Configure DbContext with provider detection (Postgres or Sqlite)
        builder.Services.AddDbContext<UmlmmDbContext>(options =>
        {
            var conn = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");

            var provider = builder.Configuration.GetValue<string>("Database:Provider")
                           ?? builder.Configuration.GetValue<string>("Provider")
                           ?? (conn.IndexOf("data source=", StringComparison.OrdinalIgnoreCase) >= 0 || conn.IndexOf(".db", StringComparison.OrdinalIgnoreCase) >= 0 ? "sqlite" : "npgsql");

            if (provider.Equals("sqlite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(conn);
            }
            else
            {
                options.UseNpgsql(conn);
            }
        });

    // Configure HttpClient with Polly policies
    builder.Services.AddHttpClient<IDanbooruApiClient, DanbooruApiClient>(client =>
    {
        client.BaseAddress = new Uri(danbooruSettings.BaseUrl);
        client.Timeout = TimeSpan.FromSeconds(danbooruSettings.TimeoutSeconds);
        
        if (!string.IsNullOrWhiteSpace(danbooruSettings.Username) && 
            !string.IsNullOrWhiteSpace(danbooruSettings.ApiKey))
        {
            var credentials = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes($"{danbooruSettings.Username}:{danbooruSettings.ApiKey}"));
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials}");
        }
    })
    .AddResilienceHandler("DanbooruResilience", (builder, context) =>
    {
        builder.AddRetry(new Polly.Retry.RetryStrategyOptions<HttpResponseMessage>
        {
            MaxRetryAttempts = danbooruSettings.RetryCount,
            BackoffType = Polly.DelayBackoffType.Exponential,
            Delay = TimeSpan.FromSeconds(1),
            UseJitter = true,
            ShouldHandle = new Polly.PredicateBuilder<HttpResponseMessage>()
                .Handle<HttpRequestException>()
                .HandleResult(r => !r.IsSuccessStatusCode && (int)r.StatusCode >= 500),
            OnRetry = args =>
            {
                Log.Warning("Retry {AttemptNumber} after {Delay}ms due to {Result}",
                    args.AttemptNumber,
                    args.RetryDelay.TotalMilliseconds,
                    args.Outcome.Result?.StatusCode.ToString() ?? args.Outcome.Exception?.Message);
                return ValueTask.CompletedTask;
            }
        });

        builder.AddCircuitBreaker(new Polly.CircuitBreaker.CircuitBreakerStrategyOptions<HttpResponseMessage>
        {
            FailureRatio = 0.5,
            MinimumThroughput = danbooruSettings.CircuitBreakerThreshold,
            BreakDuration = TimeSpan.FromSeconds(danbooruSettings.CircuitBreakerDurationSeconds),
            ShouldHandle = new Polly.PredicateBuilder<HttpResponseMessage>()
                .Handle<HttpRequestException>()
                .HandleResult(r => !r.IsSuccessStatusCode && (int)r.StatusCode >= 500),
            OnOpened = args =>
            {
                Log.Error("Circuit breaker opened for {Duration}s", args.BreakDuration.TotalSeconds);
                return ValueTask.CompletedTask;
            },
            OnClosed = args =>
            {
                Log.Information("Circuit breaker reset");
                return ValueTask.CompletedTask;
            },
            OnHalfOpened = args =>
            {
                Log.Information("Circuit breaker half-open");
                return ValueTask.CompletedTask;
            }
        });
    });

    // Register services
    builder.Services.AddScoped<DanbooruMapper>();
    builder.Services.AddScoped<DanbooruIngestionService>();
    builder.Services.AddHostedService<Worker>();

    var host = builder.Build();
    
    // Ensure database is created
    using (var scope = host.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<UmlmmDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        Log.Information("Database initialized");
    }
    
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

return 0;
