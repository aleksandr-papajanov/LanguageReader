namespace LanguageReader.Client.Features.Reading.Services;

public sealed class ReaderSessionCache
{
    private readonly Dictionary<Guid, ReadingItemDetailsDto> details = [];
    private readonly Dictionary<(Guid ReadingItemId, int PageIndex), ReadingItemContentPageDto> pages = [];

    public bool TryGetDetails(Guid readingItemId, out ReadingItemDetailsDto documentDetails)
    {
        return details.TryGetValue(readingItemId, out documentDetails!);
    }

    public void StoreDetails(Guid readingItemId, ReadingItemDetailsDto documentDetails)
    {
        details[readingItemId] = documentDetails;
    }

    public bool TryGetPage(Guid readingItemId, int pageIndex, out ReadingItemContentPageDto content)
    {
        return pages.TryGetValue((readingItemId, pageIndex), out content!);
    }

    public void StorePage(Guid readingItemId, ReadingItemContentPageDto content)
    {
        pages[(readingItemId, content.PageIndex)] = content;
    }

    public async Task WarmAsync(Guid readingItemId, string username, ReadingItemsApiClient readingItemsApi, CancellationToken cancellationToken = default)
    {
        if (details.ContainsKey(readingItemId) && pages.ContainsKey((readingItemId, 0)))
        {
            return;
        }

        var documentDetails = await readingItemsApi.GetReadingItemAsync(new GetReadingItemRequest(readingItemId, username), cancellationToken);
        var content = await readingItemsApi.GetReadingItemContentAsync(new GetReadingItemContentRequest(readingItemId, username, PageIndex: 0), cancellationToken);
        StoreDetails(readingItemId, documentDetails);
        StorePage(readingItemId, content);
    }
}
