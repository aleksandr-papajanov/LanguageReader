using LanguageReader.Infrastructure.Features.News.Services;

namespace LanguageReader.Api.Features.ReadingItems;

internal sealed class PreviewReadingItemUrlImportHandler(IArticleImportService articleImportService)
{
    public async Task<ReadingItemImportPreviewDto> HandleAsync(
        PreviewReadingItemUrlImportRequest request,
        CancellationToken ct)
    {
        var extracted = await articleImportService.ExtractWebPageAsync(
            request.Url,
            SupportedLanguages.Normalize(request.OriginalLanguage),
            ct);
        return extracted.ToImportPreviewDto();
    }
}
