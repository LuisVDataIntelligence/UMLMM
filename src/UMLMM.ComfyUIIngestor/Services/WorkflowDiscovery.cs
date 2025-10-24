using Microsoft.Extensions.Options;
using UMLMM.ComfyUIIngestor.Configuration;

namespace UMLMM.ComfyUIIngestor.Services;

public interface IWorkflowDiscovery
{
    IEnumerable<string> DiscoverWorkflowFiles();
}

public class WorkflowDiscovery : IWorkflowDiscovery
{
    private readonly ComfyUIIngestorOptions _options;

    public WorkflowDiscovery(IOptions<ComfyUIIngestorOptions> options)
    {
        _options = options.Value;
    }

    public IEnumerable<string> DiscoverWorkflowFiles()
    {
        var files = new List<string>();

        foreach (var baseDir in _options.BaseDirectories)
        {
            if (!Directory.Exists(baseDir))
            {
                continue;
            }

            foreach (var pattern in _options.IncludePatterns)
            {
                var foundFiles = Directory.GetFiles(baseDir, pattern, SearchOption.AllDirectories);
                files.AddRange(foundFiles);
            }
        }

        // Apply exclusion patterns
        if (_options.ExcludePatterns.Any())
        {
            files = files.Where(f => !_options.ExcludePatterns.Any(pattern => 
                f.Contains(pattern, StringComparison.OrdinalIgnoreCase))).ToList();
        }

        return files.Distinct();
    }
}
