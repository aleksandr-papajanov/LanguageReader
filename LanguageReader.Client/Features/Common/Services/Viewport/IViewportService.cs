namespace LanguageReader.Client.Features.Common.Services.Viewport;

public interface IViewportService
{
    ViewportState Current { get; }

    event Action<ViewportState>? Changed;

    ValueTask InitializeAsync();

    ValueTask RefreshAsync();
}
