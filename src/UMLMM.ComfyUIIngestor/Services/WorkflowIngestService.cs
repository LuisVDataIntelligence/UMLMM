using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UMLMM.ComfyUIIngestor.Configuration;
using UMLMM.Core.Data;
using UMLMM.Core.Models;

namespace UMLMM.ComfyUIIngestor.Services;

public interface IWorkflowIngestService
{
    Task<FetchRun> IngestWorkflowsAsync(CancellationToken cancellationToken);
}

public class WorkflowIngestService : IWorkflowIngestService
{
    private readonly UmlmmDbContext _context;
    private readonly IWorkflowDiscovery _discovery;
    private readonly IWorkflowParser _parser;
    private readonly ILogger<WorkflowIngestService> _logger;
    private readonly ComfyUIIngestorOptions _options;

    public WorkflowIngestService(
        UmlmmDbContext context,
        IWorkflowDiscovery discovery,
        IWorkflowParser parser,
        ILogger<WorkflowIngestService> logger,
        IOptions<ComfyUIIngestorOptions> options)
    {
        _context = context;
        _discovery = discovery;
        _parser = parser;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<FetchRun> IngestWorkflowsAsync(CancellationToken cancellationToken)
    {
        var runId = Guid.NewGuid().ToString();
        var fetchRun = new FetchRun
        {
            SourceId = _options.SourceId,
            RunId = runId,
            StartedAt = DateTime.UtcNow,
            CreatedCount = 0,
            UpdatedCount = 0,
            NoOpCount = 0,
            ErrorCount = 0
        };

        _logger.LogInformation("Starting workflow ingestion run {RunId} for source {SourceId}", 
            runId, _options.SourceId);

        try
        {
            var files = _discovery.DiscoverWorkflowFiles();
            _logger.LogInformation("Discovered {FileCount} workflow files", files.Count());

            foreach (var file in files)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Ingestion cancelled");
                    break;
                }

                try
                {
                    var workflow = _parser.ParseWorkflow(file, _options.SourceId);
                    var result = await UpsertWorkflowAsync(workflow, cancellationToken);

                    switch (result)
                    {
                        case UpsertResult.Created:
                            fetchRun.CreatedCount++;
                            _logger.LogInformation("Created workflow {ExternalId}", workflow.ExternalId);
                            break;
                        case UpsertResult.Updated:
                            fetchRun.UpdatedCount++;
                            _logger.LogInformation("Updated workflow {ExternalId}", workflow.ExternalId);
                            break;
                        case UpsertResult.NoOp:
                            fetchRun.NoOpCount++;
                            _logger.LogDebug("No changes for workflow {ExternalId}", workflow.ExternalId);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    fetchRun.ErrorCount++;
                    _logger.LogError(ex, "Error processing file {FilePath}", file);
                }
            }

            fetchRun.CompletedAt = DateTime.UtcNow;
            _logger.LogInformation(
                "Ingestion run {RunId} completed: Created={Created}, Updated={Updated}, NoOp={NoOp}, Errors={Errors}",
                runId, fetchRun.CreatedCount, fetchRun.UpdatedCount, fetchRun.NoOpCount, fetchRun.ErrorCount);
        }
        catch (Exception ex)
        {
            fetchRun.ErrorCount++;
            fetchRun.ErrorDetails = ex.ToString();
            fetchRun.CompletedAt = DateTime.UtcNow;
            _logger.LogError(ex, "Fatal error during ingestion run {RunId}", runId);
        }

        // Save fetch run
        _context.FetchRuns.Add(fetchRun);
        await _context.SaveChangesAsync(cancellationToken);

        return fetchRun;
    }

    private async Task<UpsertResult> UpsertWorkflowAsync(Workflow workflow, CancellationToken cancellationToken)
    {
        var existing = await _context.Workflows
            .FirstOrDefaultAsync(w => w.SourceId == workflow.SourceId && w.ExternalId == workflow.ExternalId, 
                cancellationToken);

        if (existing == null)
        {
            _context.Workflows.Add(workflow);
            await _context.SaveChangesAsync(cancellationToken);
            return UpsertResult.Created;
        }

        // Check if there are any changes
        if (existing.GraphJsonb == workflow.GraphJsonb && 
            existing.NodesCount == workflow.NodesCount &&
            existing.Name == workflow.Name)
        {
            return UpsertResult.NoOp;
        }

        // Update existing workflow
        existing.Name = workflow.Name;
        existing.Description = workflow.Description;
        existing.GraphJsonb = workflow.GraphJsonb;
        existing.NodesCount = workflow.NodesCount;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return UpsertResult.Updated;
    }

    private enum UpsertResult
    {
        Created,
        Updated,
        NoOp
    }
}
