using LanguageReader.Api.Features.ReadingItems;
using LanguageReader.Infrastructure.Features.News.Services;

namespace LanguageReader.Api.Features.News;

internal sealed class PreviewNewsArticleHandler(IArticleImportService articleImportService)
{
    public async Task<ReadingItemImportPreviewDto> HandleAsync(
        PreviewNewsArticleRequest request,
        CancellationToken ct)
    {
        var extracted = await articleImportService.ExtractAsync(request.SourceKey, request.Url, ct);
        return extracted.ToImportPreviewDto();
    }
}
