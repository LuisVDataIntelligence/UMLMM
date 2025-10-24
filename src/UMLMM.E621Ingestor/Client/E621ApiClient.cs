using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UMLMM.E621Ingestor.Client.DTOs;

namespace UMLMM.E621Ingestor.Client;

public interface IE621ApiClient
{
    Task<E621PostResponse?> GetPostsAsync(int page, string? tags = null, CancellationToken cancellationToken = default);
}

public class E621ApiClient : IE621ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<E621ApiClient> _logger;
    private readonly E621Options _options;

    public E621ApiClient(
        HttpClient httpClient,
        IOptions<E621Options> options,
        ILogger<E621ApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<E621PostResponse?> GetPostsAsync(
        int page, 
        string? tags = null, 
        CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>
        {
            $"limit={_options.PageSize}",
            $"page={page}"
        };

        if (!string.IsNullOrWhiteSpace(tags))
        {
            queryParams.Add($"tags={Uri.EscapeDataString(tags)}");
        }

        var query = string.Join("&", queryParams);
        var url = $"/posts.json?{query}";

        _logger.LogInformation("Fetching e621 posts: page={Page}, tags={Tags}", page, tags ?? "none");

        try
        {
            var response = await _httpClient.GetFromJsonAsync<E621PostResponse>(url, cancellationToken);
            
            if (response != null)
            {
                _logger.LogInformation("Fetched {Count} posts from e621", response.Posts.Count);
            }
            
            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching posts from e621");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching posts from e621");
            throw;
        }
    }
}

public class E621Options
{
    public const string SectionName = "E621";
    
    public string BaseUrl { get; set; } = "https://e621.net";
    public string UserAgent { get; set; } = "UMLMM/1.0 (by your_e621_username on e621)";
    public int PageSize { get; set; } = 100;
    public string? TagFilter { get; set; }
    public int RateLimitDelayMs { get; set; } = 1000;
    public int MaxRetries { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 30;
}
