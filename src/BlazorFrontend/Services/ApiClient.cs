using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Contracts.DTOs;

namespace BlazorFrontend.Services;

/// <summary>
/// HTTP client for API Gateway communication
/// </summary>
public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<PagedResultDto<ModelDto>> SearchModelsAsync(SearchRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Searching models with query: {Query}", request.Query);
            
            var queryString = BuildQueryString(request);
            var response = await _httpClient.GetAsync($"/api/models/search?{queryString}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<PagedResultDto<ModelDto>>(_jsonOptions, cancellationToken);
                return result ?? new PagedResultDto<ModelDto>();
            }

            _logger.LogWarning("Search request failed with status: {StatusCode}", response.StatusCode);
            
            // Return mock data for development/demo purposes
            return CreateMockSearchResult(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching models");
            // Return mock data on error for demo purposes
            return CreateMockSearchResult(request);
        }
    }

    public async Task<ModelDto?> GetModelAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting model with ID: {Id}", id);
            
            var response = await _httpClient.GetAsync($"/api/models/{id}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ModelDto>(_jsonOptions, cancellationToken);
            }

            _logger.LogWarning("Get model request failed with status: {StatusCode}", response.StatusCode);
            
            // Return mock data for development/demo purposes
            return CreateMockModel(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting model {Id}", id);
            // Return mock data on error for demo purposes
            return CreateMockModel(id);
        }
    }

    public async Task<ModelDto> UpdateModelAsync(int id, ModelDto model, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating model with ID: {Id}", id);
            
            var content = new StringContent(
                JsonSerializer.Serialize(model, _jsonOptions),
                Encoding.UTF8,
                "application/json");
            
            var response = await _httpClient.PutAsync($"/api/models/{id}", content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ModelDto>(_jsonOptions, cancellationToken);
                return result ?? model;
            }

            _logger.LogWarning("Update model request failed with status: {StatusCode}", response.StatusCode);
            
            // Return the input model for demo purposes
            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating model {Id}", id);
            // Return the input model on error for demo purposes
            return model;
        }
    }

    public async Task<PagedResultDto<RunDto>> GetRunsAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting runs - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
            
            var response = await _httpClient.GetAsync($"/api/runs?pageNumber={pageNumber}&pageSize={pageSize}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<PagedResultDto<RunDto>>(_jsonOptions, cancellationToken);
                return result ?? new PagedResultDto<RunDto>();
            }

            _logger.LogWarning("Get runs request failed with status: {StatusCode}", response.StatusCode);
            
            // Return mock data for development/demo purposes
            return CreateMockRunsResult(pageNumber, pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting runs");
            // Return mock data on error for demo purposes
            return CreateMockRunsResult(pageNumber, pageSize);
        }
    }

    public async Task<RunDto?> GetRunAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting run with ID: {Id}", id);
            
            var response = await _httpClient.GetAsync($"/api/runs/{id}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<RunDto>(_jsonOptions, cancellationToken);
            }

            _logger.LogWarning("Get run request failed with status: {StatusCode}", response.StatusCode);
            
            // Return mock data for development/demo purposes
            return CreateMockRun(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting run {Id}", id);
            // Return mock data on error for demo purposes
            return CreateMockRun(id);
        }
    }

    private static string BuildQueryString(SearchRequestDto request)
    {
        var queryParams = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.Query))
            queryParams.Add($"query={Uri.EscapeDataString(request.Query)}");

        if (!string.IsNullOrWhiteSpace(request.Source))
            queryParams.Add($"source={Uri.EscapeDataString(request.Source)}");

        if (request.MinRating.HasValue)
            queryParams.Add($"minRating={request.MinRating.Value}");

        if (request.Tags != null && request.Tags.Any())
            queryParams.Add($"tags={string.Join(",", request.Tags.Select(Uri.EscapeDataString))}");

        queryParams.Add($"pageNumber={request.PageNumber}");
        queryParams.Add($"pageSize={request.PageSize}");

        if (!string.IsNullOrWhiteSpace(request.SortBy))
            queryParams.Add($"sortBy={Uri.EscapeDataString(request.SortBy)}");

        if (!string.IsNullOrWhiteSpace(request.SortDirection))
            queryParams.Add($"sortDirection={Uri.EscapeDataString(request.SortDirection)}");

        return string.Join("&", queryParams);
    }

    // Mock data methods for development/demo
    private static PagedResultDto<ModelDto> CreateMockSearchResult(SearchRequestDto request)
    {
        var allModels = new List<ModelDto>
        {
            CreateMockModel(1, "Realistic Vision V5.1", "CivitAI", 4.8),
            CreateMockModel(2, "DreamShaper 8", "CivitAI", 4.7),
            CreateMockModel(3, "Anime Character LoRA", "CivitAI", 4.5),
            CreateMockModel(4, "ComfyUI Workflow Pack", "ComfyUI", 4.3),
            CreateMockModel(5, "Llama 3.1 Fine-tune", "Ollama", 4.6),
            CreateMockModel(6, "Danbooru Tag Embeddings", "Danbooru", 4.4),
            CreateMockModel(7, "E621 Style Model", "e621", 4.2),
            CreateMockModel(8, "Portrait Enhancement", "CivitAI", 4.9),
            CreateMockModel(9, "Background Remover", "ComfyUI", 4.1),
            CreateMockModel(10, "Style Transfer Pack", "CivitAI", 4.5),
        };

        // Apply filters
        var filteredModels = allModels.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            filteredModels = filteredModels.Where(m => 
                m.Name.Contains(request.Query, StringComparison.OrdinalIgnoreCase) ||
                (m.Description?.Contains(request.Query, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        if (!string.IsNullOrWhiteSpace(request.Source))
        {
            filteredModels = filteredModels.Where(m => 
                m.Source.Equals(request.Source, StringComparison.OrdinalIgnoreCase));
        }

        if (request.MinRating.HasValue)
        {
            filteredModels = filteredModels.Where(m => 
                m.Rating.HasValue && m.Rating.Value >= request.MinRating.Value);
        }

        var filteredList = filteredModels.ToList();
        var skip = (request.PageNumber - 1) * request.PageSize;
        var pagedItems = filteredList.Skip(skip).Take(request.PageSize).ToList();

        return new PagedResultDto<ModelDto>
        {
            Items = pagedItems,
            TotalCount = filteredList.Count,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    private static ModelDto CreateMockModel(int id, string? name = null, string? source = null, double? rating = null)
    {
        return new ModelDto
        {
            Id = id,
            Name = name ?? $"Model {id}",
            Description = $"This is a sample model #{id} for demonstration purposes.",
            Source = source ?? "CivitAI",
            ExternalId = $"ext-{id}",
            ModelType = "checkpoint",
            Rating = rating ?? 4.5,
            DownloadCount = 1000 + id * 100,
            Tags = new List<string> { "realistic", "portrait", "v1.5" },
            Versions = new List<ModelVersionDto>
            {
                new() { Id = id * 10 + 1, Name = "v1.0", Description = "Initial release", CreatedAt = DateTime.UtcNow.AddMonths(-2) },
                new() { Id = id * 10 + 2, Name = "v1.1", Description = "Bug fixes", CreatedAt = DateTime.UtcNow.AddMonths(-1) },
                new() { Id = id * 10 + 3, Name = "v2.0", Description = "Major update", CreatedAt = DateTime.UtcNow }
            },
            Images = new List<ModelImageDto>
            {
                new() { Id = id * 20 + 1, Url = $"https://via.placeholder.com/512x512?text=Model+{id}", IsPrimary = true, Width = 512, Height = 512 },
                new() { Id = id * 20 + 2, Url = $"https://via.placeholder.com/512x768?text=Sample+{id}", IsPrimary = false, Width = 512, Height = 768 }
            },
            CreatedAt = DateTime.UtcNow.AddMonths(-3),
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static PagedResultDto<RunDto> CreateMockRunsResult(int pageNumber, int pageSize)
    {
        var allRuns = new List<RunDto>
        {
            new() { Id = 1, Source = "CivitAI", Status = RunStatus.Completed, StartedAt = DateTime.UtcNow.AddHours(-2), CompletedAt = DateTime.UtcNow.AddHours(-1), RecordsProcessed = 150, RecordsCreated = 50, RecordsUpdated = 100, ErrorCount = 0 },
            new() { Id = 2, Source = "Danbooru", Status = RunStatus.Running, StartedAt = DateTime.UtcNow.AddMinutes(-30), RecordsProcessed = 75, RecordsCreated = 25, RecordsUpdated = 50, ErrorCount = 0 },
            new() { Id = 3, Source = "e621", Status = RunStatus.Failed, StartedAt = DateTime.UtcNow.AddHours(-4), CompletedAt = DateTime.UtcNow.AddHours(-3.5), RecordsProcessed = 20, RecordsCreated = 5, RecordsUpdated = 10, ErrorCount = 5, ErrorMessage = "API rate limit exceeded" },
            new() { Id = 4, Source = "ComfyUI", Status = RunStatus.Completed, StartedAt = DateTime.UtcNow.AddHours(-6), CompletedAt = DateTime.UtcNow.AddHours(-5), RecordsProcessed = 200, RecordsCreated = 100, RecordsUpdated = 100, ErrorCount = 0 },
            new() { Id = 5, Source = "Ollama", Status = RunStatus.Queued, StartedAt = DateTime.UtcNow.AddMinutes(-5), RecordsProcessed = 0, RecordsCreated = 0, RecordsUpdated = 0, ErrorCount = 0 },
        };

        var skip = (pageNumber - 1) * pageSize;
        var pagedItems = allRuns.Skip(skip).Take(pageSize).ToList();

        return new PagedResultDto<RunDto>
        {
            Items = pagedItems,
            TotalCount = allRuns.Count,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    private static RunDto CreateMockRun(int id)
    {
        return new RunDto
        {
            Id = id,
            Source = "CivitAI",
            Status = RunStatus.Completed,
            StartedAt = DateTime.UtcNow.AddHours(-2),
            CompletedAt = DateTime.UtcNow.AddHours(-1),
            RecordsProcessed = 100,
            RecordsCreated = 40,
            RecordsUpdated = 60,
            ErrorCount = 0
        };
    }
}
