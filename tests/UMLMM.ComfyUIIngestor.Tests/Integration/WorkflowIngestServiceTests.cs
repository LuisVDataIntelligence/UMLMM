using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Testcontainers.PostgreSql;
using UMLMM.ComfyUIIngestor.Configuration;
using UMLMM.ComfyUIIngestor.Services;
using UMLMM.Core.Data;
using UMLMM.Core.Models;

namespace UMLMM.ComfyUIIngestor.Tests.Integration;

public class WorkflowIngestServiceTests : IAsyncLifetime
{
    private PostgreSqlContainer? _postgres;
    private UmlmmDbContext? _context;
    private IWorkflowIngestService? _service;

    public async Task InitializeAsync()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .Build();

        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<UmlmmDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        _context = new UmlmmDbContext(options);
        await _context.Database.EnsureCreatedAsync();

        // Create test directory with sample workflow
        var testDir = Path.Combine(Path.GetTempPath(), "comfyui-test-" + Guid.NewGuid());
        Directory.CreateDirectory(testDir);
        
        var sampleWorkflowPath = Path.Combine(testDir, "test_workflow.json");
        File.Copy(
            Path.Combine("Resources", "sample_workflow.json"),
            sampleWorkflowPath);

        var ingestorOptions = Options.Create(new ComfyUIIngestorOptions
        {
            BaseDirectories = new List<string> { testDir },
            IncludePatterns = new List<string> { "*.json" },
            ExcludePatterns = new List<string>(),
            SourceId = "comfyui-test"
        });

        var discovery = new WorkflowDiscovery(ingestorOptions);
        var parser = new WorkflowParser();
        var logger = LoggerFactory.Create(builder => builder.AddConsole())
            .CreateLogger<WorkflowIngestService>();

        _service = new WorkflowIngestService(_context, discovery, parser, logger, ingestorOptions);
    }

    public async Task DisposeAsync()
    {
        if (_context != null)
        {
            await _context.DisposeAsync();
        }

        if (_postgres != null)
        {
            await _postgres.DisposeAsync();
        }
    }

    [Fact]
    public async Task IngestWorkflowsAsync_ShouldCreateNewWorkflow()
    {
        // Act
        var fetchRun = await _service!.IngestWorkflowsAsync(CancellationToken.None);

        // Assert
        fetchRun.Should().NotBeNull();
        fetchRun.CreatedCount.Should().Be(1);
        fetchRun.UpdatedCount.Should().Be(0);
        fetchRun.NoOpCount.Should().Be(0);
        fetchRun.ErrorCount.Should().Be(0);
        fetchRun.CompletedAt.Should().NotBeNull();

        var workflow = await _context!.Workflows.FirstOrDefaultAsync();
        workflow.Should().NotBeNull();
        workflow!.SourceId.Should().Be("comfyui-test");
        workflow.NodesCount.Should().Be(5);
    }

    [Fact]
    public async Task IngestWorkflowsAsync_ShouldBeIdempotent()
    {
        // Act - First run
        var firstRun = await _service!.IngestWorkflowsAsync(CancellationToken.None);

        // Act - Second run
        var secondRun = await _service.IngestWorkflowsAsync(CancellationToken.None);

        // Assert
        firstRun.CreatedCount.Should().Be(1);
        secondRun.CreatedCount.Should().Be(0);
        secondRun.NoOpCount.Should().Be(1);

        var workflowCount = await _context!.Workflows.CountAsync();
        workflowCount.Should().Be(1);
    }

    [Fact]
    public async Task IngestWorkflowsAsync_ShouldRecordFetchRun()
    {
        // Act
        await _service!.IngestWorkflowsAsync(CancellationToken.None);

        // Assert
        var fetchRun = await _context!.FetchRuns.FirstOrDefaultAsync();
        fetchRun.Should().NotBeNull();
        fetchRun!.SourceId.Should().Be("comfyui-test");
        fetchRun.RunId.Should().NotBeNullOrEmpty();
        fetchRun.StartedAt.Should().NotBe(default);
        fetchRun.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task IngestWorkflowsAsync_ShouldUpdateModifiedWorkflow()
    {
        // Arrange - First ingest
        await _service!.IngestWorkflowsAsync(CancellationToken.None);
        
        var workflow = await _context!.Workflows.FirstAsync();
        var originalUpdatedAt = workflow.UpdatedAt;
        
        // Modify the workflow in database to simulate a change
        workflow.NodesCount = 10;
        workflow.GraphJsonb = "{\"modified\": true}";
        await _context.SaveChangesAsync();

        // Wait a bit to ensure UpdatedAt will be different
        await Task.Delay(100);

        // Act - Second ingest (should detect the file hasn't changed and revert to file content)
        var secondRun = await _service.IngestWorkflowsAsync(CancellationToken.None);

        // Assert
        secondRun.UpdatedCount.Should().Be(1);
        
        await _context.Entry(workflow).ReloadAsync();
        workflow.NodesCount.Should().Be(5); // Should be back to original
        workflow.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }
}
