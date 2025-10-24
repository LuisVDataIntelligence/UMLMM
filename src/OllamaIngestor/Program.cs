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
    
    builder.Services.AddDbContext<UmlmmDbContext>(options =>
        options.UseNpgsql(connectionString));

    // Add repositories
    builder.Services.AddScoped<ModelRepository>();

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
