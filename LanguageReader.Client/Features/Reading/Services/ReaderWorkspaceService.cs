namespace LanguageReader.Client.Features.Reading.Services;

public sealed class ReaderWorkspaceService
{
    public event Action? Changed;
    public event Func<Task>? LeavingReader;

    public ReaderWorkspaceMode Mode { get; private set; } = ReaderWorkspaceMode.Main;

    public long ActivationVersion { get; private set; }

    public long OpenRequestVersion { get; private set; }

    public Guid? CurrentReadingItemId { get; private set; }

    public int? QueryBlock { get; private set; }

    public int? QueryOffset { get; private set; }

    private Guid? suppressedRouteReadingItemId;

    public bool IsReaderAvailable => CurrentReadingItemId.HasValue;

    public void Open(Guid readingItemId, int? block = null, int? offset = null)
    {
        suppressedRouteReadingItemId = null;
        var hasExplicitPosition = block.HasValue;

        if (CurrentReadingItemId == readingItemId)
        {
            QueryBlock = block;
            QueryOffset = offset;
            if (hasExplicitPosition)
            {
                OpenRequestVersion++;
            }

            ActivateReader();
            return;
        }

        CurrentReadingItemId = readingItemId;
        QueryBlock = block;
        QueryOffset = offset;
        OpenRequestVersion++;
        ActivateReader();
    }

    public void ShowReader()
    {
        if (!IsReaderAvailable)
        {
            return;
        }

        suppressedRouteReadingItemId = null;
        ActivateReader();
    }

    public void ShowMain()
    {
        _ = ShowMainAsync();
    }

    public async Task ShowMainAsync()
    {
        if (Mode == ReaderWorkspaceMode.Reader)
        {
            suppressedRouteReadingItemId = CurrentReadingItemId;
            await NotifyLeavingReaderAsync();
        }

        Mode = ReaderWorkspaceMode.Main;
        NotifyChanged();
    }

    public bool ShouldIgnoreRouteActivation(Guid readingItemId)
    {
        return suppressedRouteReadingItemId == readingItemId;
    }

    public void ClearSuppressedRouteActivation()
    {
        suppressedRouteReadingItemId = null;
    }

    private async Task NotifyLeavingReaderAsync()
    {
        if (LeavingReader is null)
        {
            return;
        }

        foreach (var handler in LeavingReader.GetInvocationList().Cast<Func<Task>>())
        {
            await handler();
        }
    }

    private void ActivateReader()
    {
        Mode = ReaderWorkspaceMode.Reader;
        ActivationVersion++;
        NotifyChanged();
    }

    private void NotifyChanged()
    {
        Changed?.Invoke();
    }
}
