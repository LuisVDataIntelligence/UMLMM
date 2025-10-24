using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using UMLMM.Ingestors.CivitAI.CivitAI.DTOs;

namespace UMLMM.Ingestors.CivitAI.CivitAI.Client;

public class CivitAIApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CivitAIApiClient> _logger;
    private readonly IAsyncPolicy<HttpResponseMessage> _resiliencePolicy;
    private const string BaseUrl = "https://civitai.com/api/v1";

    public CivitAIApiClient(HttpClient httpClient, ILogger<CivitAIApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        // Build resilience policy with Polly
        var retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode && (int)r.StatusCode >= 500)
            .Or<HttpRequestException>()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning("Retry {RetryCount} after {Delay}ms due to {Reason}",
                        retryCount, timespan.TotalMilliseconds, outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                });

        var circuitBreakerPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, duration) =>
                {
                    _logger.LogError("Circuit breaker opened for {Duration}s", duration.TotalSeconds);
                },
                onReset: () =>
                {
                    _logger.LogInformation("Circuit breaker reset");
                });

        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(
            TimeSpan.FromSeconds(30),
            TimeoutStrategy.Optimistic,
            onTimeoutAsync: (context, timespan, task) =>
            {
                _logger.LogWarning("Request timed out after {Timeout}s", timespan.TotalSeconds);
                return Task.CompletedTask;
            });

        _resiliencePolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);
    }

    public async Task<CivitAIModelsResponse> GetModelsAsync(
        int page = 1,
        int pageSize = 100,
        string? apiKey = null,
        CancellationToken cancellationToken = default)
    {
        var url = $"{BaseUrl}/models?page={page}&limit={pageSize}";

        _logger.LogInformation("Fetching models from CivitAI: page={Page}, pageSize={PageSize}", page, pageSize);

        var response = await _resiliencePolicy.ExecuteAsync(async ct =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (!string.IsNullOrEmpty(apiKey))
            {
                request.Headers.Add("Authorization", $"Bearer {apiKey}");
            }

            return await _httpClient.SendAsync(request, ct);
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("CivitAI API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
            throw new HttpRequestException($"CivitAI API returned {response.StatusCode}: {errorContent}");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<CivitAIModelsResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (result == null)
        {
            throw new InvalidOperationException("Failed to deserialize CivitAI API response");
        }

        _logger.LogInformation("Fetched {Count} models from page {Page}", result.Items.Count, page);

        return result;
    }
}
