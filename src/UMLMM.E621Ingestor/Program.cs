using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using UMLMM.Data;
using UMLMM.Data.Repositories;
using UMLMM.E621Ingestor.Client;
using UMLMM.E621Ingestor.Mapping;
using UMLMM.E621Ingestor.Services;
using UMLMM.E621Ingestor.Workers;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    // Add Serilog
    builder.Services.AddSerilog(Log.Logger);

    // Configure E621 options
    builder.Services.Configure<E621Options>(
        builder.Configuration.GetSection(E621Options.SectionName));

    // Configure DbContext
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    // Configure DbContext with provider detection (Postgres, Sqlite) - if provider is 'json' we'll skip DbContext
    var provider = builder.Configuration.GetValue<string>("Database:Provider")
                   ?? builder.Configuration.GetValue<string>("Provider")
                   ?? (connectionString.IndexOf("data source=", StringComparison.OrdinalIgnoreCase) >= 0 || connectionString.IndexOf(".db", StringComparison.OrdinalIgnoreCase) >= 0 ? "sqlite" : "npgsql");

    if (!provider.Equals("json", StringComparison.OrdinalIgnoreCase))
    {
        builder.Services.AddDbContext<UmlmmDbContext>(options =>
    {
            var conn = builder.Configuration.GetConnectionString("DefaultConnection")
                   ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");
        if (provider.Equals("sqlite", StringComparison.OrdinalIgnoreCase))
        {
            options.UseSqlite(conn);
        }
        else
        {
            options.UseNpgsql(conn);
        }
    });
    }

    // Configure HttpClient with Polly resilience
    var e621Options = builder.Configuration
        .GetSection(E621Options.SectionName)
        .Get<E621Options>() ?? new E621Options();

    builder.Services.AddHttpClient<IE621ApiClient, E621ApiClient>(client =>
    {
        client.BaseAddress = new Uri(e621Options.BaseUrl);
        client.DefaultRequestHeaders.Add("User-Agent", e621Options.UserAgent);
        client.Timeout = TimeSpan.FromSeconds(e621Options.TimeoutSeconds);
    })
    .AddPolicyHandler(GetRetryPolicy(e621Options))
    .AddPolicyHandler(GetCircuitBreakerPolicy());

    // Register services
    var repoProvider = builder.Configuration.GetValue<string>("Database:Provider")
                   ?? builder.Configuration.GetValue<string>("Provider");

    if (repoProvider != null && repoProvider.Equals("json", StringComparison.OrdinalIgnoreCase))
    {
        var jsonPath = builder.Configuration.GetValue<string>("Database:FilePath") ?? "umlmm.e621.json";
        builder.Services.AddScoped<IPostRepository>(_ => new JsonPostRepository(jsonPath, _.GetRequiredService<ILogger<JsonPostRepository>>()));
    }
    else
    {
        builder.Services.AddScoped<IPostRepository, PostRepository>();
    }
    builder.Services.AddScoped<IE621Mapper, E621Mapper>();
    builder.Services.AddScoped<IE621IngestorService, E621IngestorService>();

    // Register worker
    builder.Services.AddHostedService<E621IngestorWorker>();

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
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(E621Options options)
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: options.MaxRetries,
            sleepDurationProvider: retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) +
                TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                Log.Warning(
                    "Request failed with {StatusCode}. Waiting {Delay}ms before retry {Retry}/{MaxRetries}",
                    outcome.Result?.StatusCode,
                    timespan.TotalMilliseconds,
                    retryCount,
                    options.MaxRetries);
            });
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30),
            onBreak: (outcome, duration) =>
            {
                Log.Warning(
                    "Circuit breaker opened for {Duration}s due to {StatusCode}",
                    duration.TotalSeconds,
                    outcome.Result?.StatusCode);
            },
            onReset: () =>
            {
                Log.Information("Circuit breaker reset");
            });
}
