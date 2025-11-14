using Microsoft.EntityFrameworkCore;
using OllamaIngestor;
using OllamaIngestor.Services;
using Serilog;
using UMLMM.Infrastructure.Data;
using UMLMM.Infrastructure.Repositories;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting OllamaIngestor");

    var builder = Host.CreateApplicationBuilder(args);

    // Add Serilog
    builder.Services.AddSerilog();

    // Add DbContext
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    // Configure DbContext with provider detection (Postgres or Sqlite) - skip if using json provider
    var providerDb = builder.Configuration.GetValue<string>("Database:Provider")
                      ?? builder.Configuration.GetValue<string>("Provider");

    if (providerDb == null || !providerDb.Equals("json", StringComparison.OrdinalIgnoreCase))
    {
        // Configure DbContext with provider detection
        builder.Services.AddDbContext<UmlmmDbContext>(options =>
        {
            var provider = builder.Configuration.GetValue<string>("Database:Provider")
                           ?? builder.Configuration.GetValue<string>("Provider")
                           ?? (connectionString.IndexOf("data source=", StringComparison.OrdinalIgnoreCase) >= 0 || connectionString.IndexOf(".db", StringComparison.OrdinalIgnoreCase) >= 0 ? "sqlite" : "npgsql");

            if (provider.Equals("sqlite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
            }
            else
            {
                options.UseNpgsql(connectionString);
            }
        });
    }

    // Add repositories (support json file store for tests)
    var provider = builder.Configuration.GetValue<string>("Database:Provider")
                   ?? builder.Configuration.GetValue<string>("Provider");

    if (provider != null && provider.Equals("json", StringComparison.OrdinalIgnoreCase))
    {
        var jsonPath = builder.Configuration.GetValue<string>("Database:FilePath") ?? "umlmm.ollama.json";
        builder.Services.AddScoped<IModelRepository>(_ => new JsonModelRepository(jsonPath, _.GetRequiredService<ILogger<JsonModelRepository>>()));
    }
    else
    {
        builder.Services.AddScoped<IModelRepository, ModelRepository>();
    }

    // Add services
    builder.Services.AddSingleton<IOllamaClient, OllamaCliClient>();
    builder.Services.AddScoped<OllamaIngestionService>();

    // Add hosted service
    builder.Services.AddHostedService<Worker>();

    var host = builder.Build();

    // Run migrations on startup
    using (var scope = host.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<UmlmmDbContext>();
        Log.Information("Applying database migrations...");
        await dbContext.Database.MigrateAsync();
        Log.Information("Database migrations completed");
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
