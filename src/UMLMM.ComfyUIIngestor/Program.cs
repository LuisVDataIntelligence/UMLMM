using Microsoft.EntityFrameworkCore;
using Serilog;
using UMLMM.ComfyUIIngestor;
using UMLMM.ComfyUIIngestor.Configuration;
using UMLMM.ComfyUIIngestor.Services;
using UMLMM.Core.Data;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    // Configure Serilog
    builder.Services.AddSerilog();

    // Configure options
    builder.Services.Configure<ComfyUIIngestorOptions>(
        builder.Configuration.GetSection(ComfyUIIngestorOptions.SectionName));

    // Configure DbContext
    builder.Services.AddDbContext<UmlmmDbContext>(options =>
    {
            // Configure DbContext with provider detection (Postgres or Sqlite)
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

    // Register services
    builder.Services.AddScoped<IWorkflowDiscovery, WorkflowDiscovery>();
    builder.Services.AddScoped<IWorkflowParser, WorkflowParser>();
    builder.Services.AddScoped<IWorkflowIngestService, WorkflowIngestService>();

    // Register worker
    builder.Services.AddHostedService<Worker>();

    var host = builder.Build();

    // Ensure database is created
    using (var scope = host.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<UmlmmDbContext>();
        await context.Database.EnsureCreatedAsync();
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
