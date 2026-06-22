using Microsoft.JSInterop;

namespace LanguageReader.Client.Features.Common.Services.Viewport;

public sealed class ViewportService(IJSRuntime jsRuntime) : IViewportService, IAsyncDisposable
{
    private DotNetObjectReference<ViewportService>? objectReference;
    private bool initialized;

    public ViewportState Current { get; private set; } =
        CreateState(new ViewportSnapshot(
            Width: 1024,
            Height: 768,
            LayoutWidth: 1024,
            LayoutHeight: 768,
            DevicePixelRatio: 1,
            IsTouch: false));

    public event Action<ViewportState>? Changed;

    public async ValueTask InitializeAsync()
    {
        if (initialized)
        {
            return;
        }

        objectReference = DotNetObjectReference.Create(this);

        var snapshot = await jsRuntime.InvokeAsync<ViewportSnapshot>(
            "viewportService.initialize",
            objectReference);

        SetState(snapshot, notify: false);
        initialized = true;
    }

    public async ValueTask RefreshAsync()
    {
        var snapshot = await jsRuntime.InvokeAsync<ViewportSnapshot>("viewportService.getSnapshot");
        SetState(snapshot, notify: true);
    }

    [JSInvokable]
    public void OnViewportChanged(ViewportSnapshot snapshot)
    {
        SetState(snapshot, notify: true);
    }

    private void SetState(ViewportSnapshot snapshot, bool notify)
    {
        var next = CreateState(snapshot);
        if (next == Current)
        {
            return;
        }

        Current = next;

        if (notify)
        {
            Changed?.Invoke(Current);
        }
    }

    private static ViewportState CreateState(ViewportSnapshot snapshot)
    {
        return new ViewportState(
            snapshot.Width,
            snapshot.Height,
            snapshot.LayoutWidth,
            snapshot.LayoutHeight,
            snapshot.DevicePixelRatio,
            snapshot.IsTouch,
            GetBreakpoint(snapshot.Width));
    }

    private static ViewportBreakpoint GetBreakpoint(int width)
    {
        if (width < 768)
        {
            return ViewportBreakpoint.Mobile;
        }

        if (width < 1100)
        {
            return ViewportBreakpoint.Tablet;
        }

        return ViewportBreakpoint.Desktop;
    }

    public async ValueTask DisposeAsync()
    {
        if (objectReference is null)
        {
            return;
        }

        try
        {
            await jsRuntime.InvokeVoidAsync("viewportService.dispose");
        }
        catch
        {
            // Ignore JS disconnect during app shutdown.
        }

        objectReference.Dispose();
    }

    public sealed record ViewportSnapshot(
        int Width,
        int Height,
        int LayoutWidth,
        int LayoutHeight,
        double DevicePixelRatio,
        bool IsTouch);
}
