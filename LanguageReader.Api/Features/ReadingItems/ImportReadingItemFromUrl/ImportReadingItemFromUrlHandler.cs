using LanguageReader.Infrastructure.Features.News.Services;

namespace LanguageReader.Api.Features.ReadingItems;

internal sealed class ImportReadingItemFromUrlHandler(
    IArticleImportService articleImportService,
    ReadingItemImportWriter importWriter)
{
    public async Task<ReadingItemDetailsDto> HandleAsync(
        ImportReadingItemFromUrlRequest request,
        CancellationToken ct)
    {
        var username = UsernameHelper.Require(request.Username);
        var originalLanguage = SupportedLanguages.Normalize(request.OriginalLanguage);
        var extracted = await articleImportService.ExtractWebPageAsync(request.Url, originalLanguage, ct);

        return await importWriter.SaveArticleAsync(
            username,
            extracted,
            request.Title,
            originalLanguage,
            ct);
    }
}
