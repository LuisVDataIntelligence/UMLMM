using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UMLMM.Data.Repositories;
using UMLMM.E621Ingestor.Client;
using UMLMM.E621Ingestor.Mapping;

namespace UMLMM.E621Ingestor.Services;

public interface IE621IngestorService
{
    Task IngestAsync(CancellationToken cancellationToken = default);
}

public class E621IngestorService : IE621IngestorService
{
    private readonly IE621ApiClient _apiClient;
    private readonly IPostRepository _postRepository;
    private readonly IE621Mapper _mapper;
    private readonly ILogger<E621IngestorService> _logger;
    private readonly E621Options _options;

    public E621IngestorService(
        IE621ApiClient apiClient,
        IPostRepository postRepository,
        IE621Mapper mapper,
        IOptions<E621Options> options,
        ILogger<E621IngestorService> logger)
    {
        _apiClient = apiClient;
        _postRepository = postRepository;
        _mapper = mapper;
        _logger = logger;
        _options = options.Value;
    }

    public async Task IngestAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting e621 ingestion");

        var source = await _postRepository.EnsureSourceAsync("e621", cancellationToken);
        var fetchRun = await _postRepository.CreateFetchRunAsync(source.Id, cancellationToken);

        try
        {
            var page = 1;
            var hasMorePages = true;
            var totalFetched = 0;
            var totalCreated = 0;
            var totalUpdated = 0;

            while (hasMorePages && !cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Fetching page {Page}", page);

                var response = await _apiClient.GetPostsAsync(page, _options.TagFilter, cancellationToken);

                if (response == null || !response.Posts.Any())
                {
                    _logger.LogInformation("No more posts found, stopping ingestion");
                    hasMorePages = false;
                    break;
                }

                totalFetched += response.Posts.Count;

                foreach (var apiPost in response.Posts)
                {
                    try
                    {
                        var existingPost = await _postRepository.GetPostByExternalIdAsync(
                            source.Id,
                            apiPost.Id.ToString(),
                            cancellationToken);

                        var post = _mapper.MapToPost(apiPost, source.Id);
                        var tags = _mapper.ExtractTags(apiPost);
                        var image = _mapper.MapToImage(apiPost);

                        await _postRepository.UpsertPostAsync(post, tags, image, cancellationToken);

                        if (existingPost != null)
                        {
                            totalUpdated++;
                        }
                        else
                        {
                            totalCreated++;
                        }

                        _logger.LogDebug("Processed post {PostId}", apiPost.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing post {PostId}", apiPost.Id);
                    }
                }

                page++;

                // Rate limiting
                if (hasMorePages)
                {
                    _logger.LogDebug("Rate limiting: waiting {Delay}ms", _options.RateLimitDelayMs);
                    await Task.Delay(_options.RateLimitDelayMs, cancellationToken);
                }
            }

            fetchRun.PostsFetched = totalFetched;
            fetchRun.PostsCreated = totalCreated;
            fetchRun.PostsUpdated = totalUpdated;
            fetchRun.Success = true;
            fetchRun.CompletedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Ingestion completed: Fetched={Fetched}, Created={Created}, Updated={Updated}",
                totalFetched, totalCreated, totalUpdated);
        }
        catch (Exception ex)
        {
            fetchRun.Success = false;
            fetchRun.ErrorMessage = ex.Message;
            fetchRun.CompletedAt = DateTime.UtcNow;

            _logger.LogError(ex, "Error during ingestion");
            throw;
        }
        finally
        {
            await _postRepository.UpdateFetchRunAsync(fetchRun, cancellationToken);
        }
    }
}
