namespace LanguageReader.Client.Features.Reading.Services;

public sealed class ReaderSessionCache
{
    private readonly Dictionary<Guid, CachedReaderDocument> documents = [];

    public bool TryGet(Guid readingItemId, out CachedReaderDocument document)
    {
        return documents.TryGetValue(readingItemId, out document!);
    }

    public void Store(Guid readingItemId, ReadingItemDetailsDto details, ReadingItemContentDto content)
    {
        documents[readingItemId] = new CachedReaderDocument(details, content);
    }

    public async Task WarmAsync(Guid readingItemId, string username, ReadingItemsApiClient readingItemsApi, CancellationToken cancellationToken = default)
    {
        if (documents.ContainsKey(readingItemId))
        {
            return;
        }

        var details = await readingItemsApi.GetReadingItemAsync(new GetReadingItemRequest(readingItemId, username), cancellationToken);
        var content = await readingItemsApi.GetReadingItemContentAsync(new GetReadingItemContentRequest(readingItemId, username), cancellationToken);
        Store(readingItemId, details, content);
    }
}

public sealed record CachedReaderDocument(
    ReadingItemDetailsDto Details,
    ReadingItemContentDto Content);
