using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace UMLMM.DanbooruIngestor.Danbooru;

public class DanbooruApiClient : IDanbooruApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DanbooruApiClient> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public DanbooruApiClient(HttpClient httpClient, ILogger<DanbooruApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<DanbooruPostDto>> GetPostsAsync(
        int page, 
        int limit, 
        string? tags = null, 
        CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>
        {
            $"page={page}",
            $"limit={limit}"
        };
        
        if (!string.IsNullOrWhiteSpace(tags))
        {
            queryParams.Add($"tags={Uri.EscapeDataString(tags)}");
        }
        
        var url = $"/posts.json?{string.Join("&", queryParams)}";
        
        _logger.LogDebug("Fetching posts from Danbooru: {Url}", url);
        
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var posts = JsonSerializer.Deserialize<List<DanbooruPostDto>>(content, JsonOptions) 
                    ?? new List<DanbooruPostDto>();
        
        _logger.LogInformation("Fetched {Count} posts from page {Page}", posts.Count, page);
        
        return posts;
    }
}
