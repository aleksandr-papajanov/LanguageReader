using LanguageReader.Api.Features.ReadingItems;
using LanguageReader.Infrastructure.Features.News.Services;

namespace LanguageReader.Api.Features.News;

internal sealed class ImportNewsArticleHandler(
    NewsArticleImportService newsArticleImport,
    ReadingItemApiUrlBuilder apiUrls)
{
    public async Task<ReadingItemDetailsDto> HandleAsync(ImportNewsArticleRequest request, CancellationToken ct)
    {
        var username = UsernameHelper.Require(request.Username);
        var readingItem = await newsArticleImport.ImportAsync(
            username,
            request.SourceKey,
            request.Url,
            ct);

        return readingItem.ToReadingItemDetailsDto(username, apiUrls);
    }
}
