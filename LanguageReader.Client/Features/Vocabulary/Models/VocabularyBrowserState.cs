namespace LanguageReader.Client.Features.Vocabulary.Models;

public sealed class VocabularyBrowserState
{
    private const int InitialRenderCount = 24;
    private const int PageSize = 16;

    public List<VocabularyEntryDto> Entries { get; } = [];

    public List<AppSelectOption<Guid?>> BookOptions { get; } = [];

    public bool IsLoading { get; private set; }

    public bool IsLoadingMore { get; private set; }

    public Guid? SelectedBookId { get; private set; }

    public int RenderedCount { get; private set; } = InitialRenderCount;

    public IReadOnlyList<VocabularyEntryDto> VisibleEntries =>
        Entries
            .Where(entry => !SelectedBookId.HasValue || entry.ReadingItemId == SelectedBookId.Value)
            .OrderByDescending(entry => entry.CreatedAtUtc)
            .Take(RenderedCount)
            .ToList();

    public bool HasMoreEntries =>
        Entries.Count(entry => !SelectedBookId.HasValue || entry.ReadingItemId == SelectedBookId.Value) > RenderedCount;

    public async Task LoadAsync(
        VocabularyApiClient api,
        string username,
        CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        try
        {
            Entries.Clear();
            Entries.AddRange(await api.GetVocabularyAsync(
                new GetVocabularyRequest(username, false),
                cancellationToken));

            BuildBookOptions();
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void OnBookChanged(Guid? bookId)
    {
        SelectedBookId = bookId;
        RenderedCount = InitialRenderCount;
    }

    public void LoadMore()
    {
        if (!HasMoreEntries)
        {
            return;
        }

        IsLoadingMore = true;
        RenderedCount += PageSize;
        IsLoadingMore = false;
    }

    public async Task HideEntryAsync(
        VocabularyApiClient api,
        VocabularyEntryDto entry,
        string username,
        CancellationToken cancellationToken = default)
    {
        var updated = await api.UpdateVocabularyVisibilityAsync(
            new UpdateVocabularyVisibilityRequest(entry.Id, username, false),
            cancellationToken);

        Entries.RemoveAll(item => item.Id == entry.Id);
        Entries.Add(updated);
        BuildBookOptions();
    }

    private void BuildBookOptions()
    {
        BookOptions.Clear();
        BookOptions.Add(new AppSelectOption<Guid?>(null, "All books"));

        foreach (var item in Entries
                     .Select(entry => new { entry.ReadingItemId, entry.ReadingItemTitle })
                     .DistinctBy(item => item.ReadingItemId)
                     .OrderBy(item => item.ReadingItemTitle))
        {
            BookOptions.Add(new AppSelectOption<Guid?>(item.ReadingItemId, item.ReadingItemTitle));
        }
    }
}
