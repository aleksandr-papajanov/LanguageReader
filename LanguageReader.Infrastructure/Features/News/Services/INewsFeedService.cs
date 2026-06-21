using LanguageReader.Infrastructure.Features.News.Models;

namespace LanguageReader.Infrastructure.Features.News.Services;

public interface INewsFeedService
{
    Task<IReadOnlyList<FetchedNewsArticle>> FetchAsync(string sourceKey, CancellationToken cancellationToken = default);
}
