using LanguageReader.Infrastructure.Features.ReadingItems.Services;

namespace LanguageReader.Api.Features.ReadingItems;

internal sealed class GetReadingItemHandler(
    ReadingItemAccessService readingItems,
    ReadingItemApiUrlBuilder apiUrls)
{
    public async Task<ReadingItemDetailsDto> HandleAsync(GetReadingItemRequest request, CancellationToken ct)
    {
        var item = await readingItems.LoadDetailsReadableReadOnlyAsync(request.ReadingItemId, request.Username, ct);

        return item.ToReadingItemDetailsDto(request.Username, apiUrls);
    }
}
