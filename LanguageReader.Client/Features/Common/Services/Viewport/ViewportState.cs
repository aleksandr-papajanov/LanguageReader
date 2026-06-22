namespace LanguageReader.Client.Features.Common.Services.Viewport;

public sealed record ViewportState(
    int Width,
    int Height,
    int LayoutWidth,
    int LayoutHeight,
    double DevicePixelRatio,
    bool IsTouch,
    ViewportBreakpoint Breakpoint)
{
    public bool IsMobile => Breakpoint == ViewportBreakpoint.Mobile;
    public bool IsTablet => Breakpoint == ViewportBreakpoint.Tablet;
    public bool IsDesktop => Breakpoint == ViewportBreakpoint.Desktop;
    public bool IsPortrait => Height >= Width;
    public bool IsLandscape => Width > Height;
    public string BreakpointName => Breakpoint.ToString();
}
