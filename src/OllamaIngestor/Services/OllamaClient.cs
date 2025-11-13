using System.Diagnostics;
using System.Text.Json;
using OllamaIngestor.Models;

namespace OllamaIngestor.Services;

public interface IOllamaClient
{
    Task<List<OllamaModel>> ListModelsAsync(CancellationToken cancellationToken = default);
    Task<OllamaModelShow?> ShowModelAsync(string modelName, CancellationToken cancellationToken = default);
}

public class OllamaCliClient : IOllamaClient
{
    private readonly ILogger<OllamaCliClient> _logger;
    private readonly string _ollamaCommand;

    public OllamaCliClient(ILogger<OllamaCliClient> logger, IConfiguration configuration)
    {
        _logger = logger;
        _ollamaCommand = configuration["Ollama:CommandPath"] ?? "ollama";
    }

    public async Task<List<OllamaModel>> ListModelsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var output = await ExecuteCommandAsync("list --format json", cancellationToken);
            
            if (string.IsNullOrWhiteSpace(output))
            {
                _logger.LogWarning("Empty output from 'ollama list' command");
                return new List<OllamaModel>();
            }

            // Try to parse as OllamaListResponse
            try
            {
                var response = JsonSerializer.Deserialize<OllamaListResponse>(output, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });
                return response?.Models ?? new List<OllamaModel>();
            }
            catch (JsonException)
            {
                // If that fails, try parsing as array directly
                var models = JsonSerializer.Deserialize<List<OllamaModel>>(output, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });
                return models ?? new List<OllamaModel>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list Ollama models");
            return new List<OllamaModel>();
        }
    }

    public async Task<OllamaModelShow?> ShowModelAsync(string modelName, CancellationToken cancellationToken = default)
    {
        try
        {
            var output = await ExecuteCommandAsync($"show {modelName} --format json", cancellationToken);
            
            if (string.IsNullOrWhiteSpace(output))
            {
                _logger.LogWarning("Empty output from 'ollama show {ModelName}' command", modelName);
                return null;
            }

            var model = JsonSerializer.Deserialize<OllamaModelShow>(output, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
            
            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show Ollama model {ModelName}", modelName);
            return null;
        }
    }

    private async Task<string> ExecuteCommandAsync(string arguments, CancellationToken cancellationToken)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = _ollamaCommand,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processInfo };
        
        var outputBuilder = new System.Text.StringBuilder();
        var errorBuilder = new System.Text.StringBuilder();

        process.OutputDataReceived += (sender, args) =>
        {
            if (args.Data != null)
            {
                outputBuilder.AppendLine(args.Data);
            }
        };

        process.ErrorDataReceived += (sender, args) =>
        {
            if (args.Data != null)
            {
                errorBuilder.AppendLine(args.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            var error = errorBuilder.ToString();
            _logger.LogError("Ollama command failed with exit code {ExitCode}: {Error}", process.ExitCode, error);
            throw new InvalidOperationException($"Ollama command failed: {error}");
        }

        return outputBuilder.ToString();
    }
}
