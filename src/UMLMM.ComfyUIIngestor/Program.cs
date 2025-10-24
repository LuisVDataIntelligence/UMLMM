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
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        options.UseNpgsql(connectionString);
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
