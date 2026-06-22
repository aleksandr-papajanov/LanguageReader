using Microsoft.JSInterop;

namespace LanguageReader.Client.Features.Common.Services;

public sealed class ThemeService(IJSRuntime jsRuntime)
{
    public AppTheme CurrentTheme { get; private set; } = AppTheme.Light;

    public event Action? Changed;

    public async Task InitializeAsync()
    {
        var themeName = await jsRuntime.InvokeAsync<string>("languageReaderTheme.initialize");
        CurrentTheme = ParseTheme(themeName);
        Changed?.Invoke();
    }

    public Task ToggleAsync()
    {
        return SetThemeAsync(GetNextTheme(CurrentTheme));
    }

    public async Task SetThemeAsync(AppTheme theme)
    {
        CurrentTheme = theme;
        await jsRuntime.InvokeVoidAsync("languageReaderTheme.set", ToStorageValue(theme));
        Changed?.Invoke();
    }

    public static string GetDisplayName(AppTheme theme)
    {
        return theme switch
        {
            AppTheme.Light => "Light",
            AppTheme.Warm => "Warm",
            AppTheme.Dark => "Dark",
            _ => "Light"
        };
    }

    public static string GetIcon(AppTheme theme)
    {
        return theme switch
        {
            AppTheme.Light => "light_mode",
            AppTheme.Warm => "wb_sunny",
            AppTheme.Dark => "dark_mode",
            _ => "light_mode"
        };
    }

    private static AppTheme GetNextTheme(AppTheme theme)
    {
        return theme switch
        {
            AppTheme.Light => AppTheme.Warm,
            AppTheme.Warm => AppTheme.Dark,
            AppTheme.Dark => AppTheme.Light,
            _ => AppTheme.Light
        };
    }

    private static AppTheme ParseTheme(string? themeName)
    {
        return string.Equals(themeName, "warm", StringComparison.OrdinalIgnoreCase)
            ? AppTheme.Warm
            : string.Equals(themeName, "dark", StringComparison.OrdinalIgnoreCase)
                ? AppTheme.Dark
                : AppTheme.Light;
    }

    private static string ToStorageValue(AppTheme theme)
    {
        return theme switch
        {
            AppTheme.Warm => "warm",
            AppTheme.Dark => "dark",
            _ => "light"
        };
    }
}
