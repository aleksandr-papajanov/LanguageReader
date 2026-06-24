using LanguageReader.Infrastructure.Features.News.Models;

namespace LanguageReader.Infrastructure.Features.News.Services;

public interface IArticleImportService
{
    Task<ExtractedArticleContent> ExtractAsync(string sourceKey, string url, CancellationToken cancellationToken = default);

    Task<ExtractedArticleContent> ExtractWebPageAsync(string url, string originalLanguage, CancellationToken cancellationToken = default);

    Task<NewsArticlePreviewMetadata?> TryExtractPreviewAsync(string sourceKey, string url, CancellationToken cancellationToken = default);
}
