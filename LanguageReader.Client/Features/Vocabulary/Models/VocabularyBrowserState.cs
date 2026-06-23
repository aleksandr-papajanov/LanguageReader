namespace LanguageReader.Client.Features.Vocabulary.Models;

public sealed class VocabularyBrowserState
{
    private const int InitialRenderCount = 24;
    private const int PageSize = 16;

    public List<VocabularyEntryDto> Entries { get; } = [];

    public List<AppSelectOption<Guid?>> ReadingItemOptions { get; } = [];

    public bool IsLoading { get; private set; }

    public bool IsLoadingMore { get; private set; }

    public Guid? SelectedReadingItemId { get; private set; }

    public int RenderedCount { get; private set; } = InitialRenderCount;

    public IReadOnlyList<VocabularyEntryDto> VisibleEntries =>
        Entries
            .Where(entry => !SelectedReadingItemId.HasValue || entry.ReadingItemId == SelectedReadingItemId.Value)
            .OrderByDescending(entry => entry.CreatedAtUtc)
            .Take(RenderedCount)
            .ToList();

    public bool HasMoreEntries =>
        Entries.Count(entry => !SelectedReadingItemId.HasValue || entry.ReadingItemId == SelectedReadingItemId.Value) > RenderedCount;

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

            BuildReadingItemOptions();
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void OnReadingItemChanged(Guid? readingItemId)
    {
        SelectedReadingItemId = readingItemId;
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
        await api.UpdateVocabularyVisibilityAsync(
            new UpdateVocabularyVisibilityRequest(entry.Id, username, false),
            cancellationToken);

        Entries.RemoveAll(item => item.Id == entry.Id);
        BuildReadingItemOptions();
    }

    private void BuildReadingItemOptions()
    {
        ReadingItemOptions.Clear();
        ReadingItemOptions.Add(new AppSelectOption<Guid?>(null, "All reading items"));

        foreach (var item in Entries
                     .Select(entry => new { entry.ReadingItemId, entry.ReadingItemTitle })
                     .Where(item => item.ReadingItemId.HasValue)
                     .DistinctBy(item => item.ReadingItemId)
                     .OrderBy(item => item.ReadingItemTitle))
        {
            ReadingItemOptions.Add(new AppSelectOption<Guid?>(item.ReadingItemId, item.ReadingItemTitle));
        }
    }
}
