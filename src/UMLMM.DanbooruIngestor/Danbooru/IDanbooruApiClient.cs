namespace UMLMM.DanbooruIngestor.Danbooru;

public interface IDanbooruApiClient
{
    Task<List<DanbooruPostDto>> GetPostsAsync(int page, int limit, string? tags = null, CancellationToken cancellationToken = default);
}
