using LanguageReader.Infrastructure.Features.News.Models;
using LanguageReader.Infrastructure.Features.ReadingItems.Parsing.Models;
using LanguageReader.Infrastructure.Features.ReadingItems.Services;

namespace LanguageReader.Api.Features.ReadingItems;

internal sealed class ReadingItemImportWriter(
    ReadingItemImportService importService,
    ReadingItemApiUrlBuilder apiUrls)
{
    public async Task<ReadingItemDetailsDto> SaveBookAsync(
        string username,
        string? requestedTitle,
        string? requestedOriginalLanguage,
        string fallbackFileName,
        ParsedReadingDocument parsedDocument,
        CancellationToken ct)
    {
        var readingItem = await importService.SaveBookAsync(
            username,
            requestedTitle,
            requestedOriginalLanguage,
            fallbackFileName,
            parsedDocument,
            ct);

        return readingItem.ToReadingItemDetailsDto(username, apiUrls);
    }

    public async Task<ReadingItemDetailsDto> SaveArticleAsync(
        string username,
        ExtractedArticleContent extracted,
        string? requestedTitle,
        string? requestedOriginalLanguage,
        CancellationToken ct)
    {
        var readingItem = await importService.SaveArticleAsync(
            username,
            extracted,
            requestedTitle,
            requestedOriginalLanguage,
            ct);

        return readingItem.ToReadingItemDetailsDto(username, apiUrls);
    }
}
