using LanguageReader.Infrastructure.Features.ReadingItems.Services;

namespace LanguageReader.Api.Features.ReadingItems;

internal sealed class GetReadingItemContentHandler(
    ReadingItemAccessService readingItems,
    IReadingItemContentService readingItemContentService)
{
    public async Task<ReadingItemContentPageDto> HandleAsync(GetReadingItemContentRequest request, CancellationToken ct)
    {
        var item = await readingItems.LoadReadableReadOnlyAsync(request.ReadingItemId, request.Username, ct);

        return await readingItemContentService.LoadPageAsync(item, request, ct);
    }
}
