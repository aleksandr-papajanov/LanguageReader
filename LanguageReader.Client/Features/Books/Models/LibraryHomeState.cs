namespace LanguageReader.Client.Features.Books.Models;

public sealed class LibraryHomeState
{
    public static readonly AppTabItem[] Tabs =
    [
        new("reading", "Reading", "history_edu"),
        new("my-books", "Library", "book_2"),
        new("public", "Public", "public")
    ];

    public List<ReadingItemSummaryDto> Items { get; } = [];

    public string? Error { get; private set; }

    public bool IsLoading { get; private set; }

    public string ActiveTab { get; private set; } = "reading";

    public void SetInitialTab(string? tab)
    {
        ActiveTab = NormalizeTab(tab);
    }

    public async Task LoadAsync(
        string username,
        ReadingItemsApiClient readingItemsApi,
        ReaderSessionCache readerCache,
        CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        Error = null;

        try
        {
            Items.Clear();
            Items.AddRange(await readingItemsApi.GetReadingItemsAsync(
                new GetReadingItemsRequest(username, Collection: ReadingItemCollectionFilter.Library),
                cancellationToken));

            _ = WarmReaderCacheAsync(readerCache, readingItemsApi, username);
        }
        catch (Exception exception)
        {
            Error = exception.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task TogglePublicAsync(
        ReadingItemSummaryDto item,
        string username,
        ReadingItemsApiClient readingItemsApi,
        CancellationToken cancellationToken = default)
    {
        await readingItemsApi.SetVisibilityAsync(
            new UpdateReadingItemVisibilityRequest(item.Id, username, !item.IsPublic),
            cancellationToken);

        await ReloadAsync(username, readingItemsApi, cancellationToken);
    }

    public async Task DeleteAsync(
        Guid id,
        string username,
        ReadingItemsApiClient readingItemsApi,
        CancellationToken cancellationToken = default)
    {
        await readingItemsApi.DeleteReadingItemAsync(new DeleteReadingItemRequest(id, username), cancellationToken);
        Items.RemoveAll(item => item.Id == id);
        await ReloadAsync(username, readingItemsApi, cancellationToken);
    }

    public string SelectTab(string tab)
    {
        ActiveTab = NormalizeTab(tab);
        return BuildLibraryRoute(ActiveTab);
    }

    public static string NormalizeTab(string? tab)
    {
        return tab?.Trim().ToLowerInvariant() switch
        {
            "reading" => "reading",
            "my-books" => "my-books",
            "public" => "public",
            _ => "reading"
        };
    }

    private async Task ReloadAsync(
        string username,
        ReadingItemsApiClient readingItemsApi,
        CancellationToken cancellationToken)
    {
        Items.Clear();
        Items.AddRange(await readingItemsApi.GetReadingItemsAsync(
            new GetReadingItemsRequest(username, Collection: ReadingItemCollectionFilter.Library),
            cancellationToken));
    }

    private async Task WarmReaderCacheAsync(
        ReaderSessionCache readerCache,
        ReadingItemsApiClient readingItemsApi,
        string username)
    {
        try
        {
            foreach (var item in Items.Where(item => item.CanContinue).Take(2))
            {
                await readerCache.WarmAsync(item.Id, username, readingItemsApi);
            }
        }
        catch
        {
        }
    }

    private static string BuildLibraryRoute(string tab)
    {
        return tab == "reading" ? "/" : $"/?tab={tab}";
    }
}
