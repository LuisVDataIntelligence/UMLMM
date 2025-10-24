using System.Text.Json;
using UMLMM.Core.Models;

namespace UMLMM.ComfyUIIngestor.Services;

public interface IWorkflowParser
{
    Workflow ParseWorkflow(string filePath, string sourceId);
}

public class WorkflowParser : IWorkflowParser
{
    public Workflow ParseWorkflow(string filePath, string sourceId)
    {
        var json = File.ReadAllText(filePath);
        var jsonDocument = JsonDocument.Parse(json);
        
        // Count nodes in the workflow graph
        int nodesCount = 0;
        if (jsonDocument.RootElement.TryGetProperty("nodes", out var nodes) && nodes.ValueKind == JsonValueKind.Array)
        {
            nodesCount = nodes.GetArrayLength();
        }
        else if (jsonDocument.RootElement.ValueKind == JsonValueKind.Object)
        {
            // ComfyUI workflows often store nodes as object properties with numeric keys
            // Count only properties with numeric keys as nodes
            nodesCount = jsonDocument.RootElement.EnumerateObject()
                .Count(p => int.TryParse(p.Name, out _));
        }

        // Extract metadata if available
        string? name = null;
        string? description = null;
        
        if (jsonDocument.RootElement.TryGetProperty("extra", out var extra))
        {
            if (extra.TryGetProperty("ds", out var ds))
            {
                if (ds.TryGetProperty("workflow_name", out var workflowName))
                {
                    name = workflowName.GetString();
                }
            }
        }

        // Use file name as fallback for name
        if (string.IsNullOrEmpty(name))
        {
            name = Path.GetFileNameWithoutExtension(filePath);
        }

        var externalId = Path.GetFileName(filePath);
        var now = DateTime.UtcNow;

        return new Workflow
        {
            SourceId = sourceId,
            ExternalId = externalId,
            Name = name,
            Description = description,
            GraphJsonb = json,
            NodesCount = nodesCount,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
