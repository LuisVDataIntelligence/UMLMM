using UMLMM.Core.Domain.Entities;

namespace UMLMM.Data.Repositories;

public interface IPostRepository
{
    Task<Source> EnsureSourceAsync(string sourceName, CancellationToken cancellationToken = default);
    Task<FetchRun> CreateFetchRunAsync(int sourceId, CancellationToken cancellationToken = default);
    Task UpdateFetchRunAsync(FetchRun fetchRun, CancellationToken cancellationToken = default);
    Task<Post?> GetPostByExternalIdAsync(int sourceId, string externalId, CancellationToken cancellationToken = default);
    Task<Tag?> GetTagByNameAsync(string name, CancellationToken cancellationToken = default);
    Task UpsertPostAsync(Post post, List<Tag> tags, Image image, CancellationToken cancellationToken = default);
}
