namespace LanguageReader.Client.Features.Reading.Services;

public sealed class ReaderWorkspaceService
{
    public event Action? Changed;

    public ReaderWorkspaceMode Mode { get; private set; } = ReaderWorkspaceMode.Main;

    public Guid? CurrentReadingItemId { get; private set; }

    public int? QueryBlock { get; private set; }

    public int? QueryOffset { get; private set; }

    public bool IsReaderAvailable => CurrentReadingItemId.HasValue;

    public void Open(Guid readingItemId, int? block = null, int? offset = null)
    {
        CurrentReadingItemId = readingItemId;
        QueryBlock = block;
        QueryOffset = offset;
        Mode = ReaderWorkspaceMode.Reader;
        NotifyChanged();
    }

    public void ShowReader()
    {
        if (!IsReaderAvailable)
        {
            return;
        }

        Mode = ReaderWorkspaceMode.Reader;
        NotifyChanged();
    }

    public void ShowMain()
    {
        Mode = ReaderWorkspaceMode.Main;
        NotifyChanged();
    }

    private void NotifyChanged()
    {
        Changed?.Invoke();
    }
}
