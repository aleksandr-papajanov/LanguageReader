using Microsoft.JSInterop;

namespace LanguageReader.Client.Features.Users.Services;

/// <summary>
/// Temporary username-only client session for local development.
/// </summary>
public sealed class UserSession(IJSRuntime jsRuntime)
{
    private const string StorageKey = "languageReader.username";
    private const string LastReaderRouteKey = "languageReader.lastReaderRoute";

    /// <summary>
    /// Current username, if signed in.
    /// </summary>
    public string? Username { get; private set; }

    /// <summary>
    /// Last opened reader route.
    /// </summary>
    public string LastReaderRoute { get; private set; } = "/";

    /// <summary>
    /// Indicates whether a username is present.
    /// </summary>
    public bool IsSignedIn => !string.IsNullOrWhiteSpace(Username);

    /// <summary>
    /// Raised when the session changes.
    /// </summary>
    public event Action? Changed;

    /// <summary>
    /// Loads the session from browser local storage.
    /// </summary>
    public async Task InitializeAsync()
    {
        Username = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
        LastReaderRoute = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", LastReaderRouteKey) ?? "/";
        Changed?.Invoke();
    }

    /// <summary>
    /// Stores a temporary username session.
    /// </summary>
    /// <param name="username">The username.</param>
    public async Task SignInAsync(string username)
    {
        Username = username.Trim().ToLowerInvariant();
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, Username);
        Changed?.Invoke();
    }

    /// <summary>
    /// Clears the temporary username session.
    /// </summary>
    public async Task SignOutAsync()
    {
        Username = null;
        await jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey);
        Changed?.Invoke();
    }

    /// <summary>
    /// Stores the last reader route for quick return navigation.
    /// </summary>
    /// <param name="route">Reader route.</param>
    public async Task SetLastReaderRouteAsync(string route)
    {
        if (string.IsNullOrWhiteSpace(route))
        {
            return;
        }

        LastReaderRoute = route.StartsWith('/') ? route : $"/{route}";
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", LastReaderRouteKey, LastReaderRoute);
        Changed?.Invoke();
    }
}

