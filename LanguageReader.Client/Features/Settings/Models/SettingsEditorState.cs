namespace LanguageReader.Client.Features.Settings.Models;

public sealed class SettingsEditorState
{
    public string NativeLanguage { get; private set; } = SupportedLanguages.Russian;

    public bool IsLoading { get; private set; }

    public string? Message { get; private set; }

    public string? Error { get; private set; }

    public async Task LoadAsync(
        SettingsApiClient api,
        string username,
        CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        try
        {
            var settings = await api.GetSettingsAsync(
                new GetUserSettingsRequest(username),
                cancellationToken);

            NativeLanguage = SupportedLanguages.Normalize(settings.NativeLanguage);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void OnNativeLanguageChanged(string value)
    {
        NativeLanguage = SupportedLanguages.Normalize(value);
    }

    public async Task SaveAsync(
        SettingsApiClient api,
        string username,
        CancellationToken cancellationToken = default)
    {
        Error = null;
        Message = null;

        try
        {
            await api.UpdateSettingsAsync(
                new UpdateUserSettingsRequest(username, NativeLanguage),
                cancellationToken);

            Message = "Settings saved.";
        }
        catch (Exception exception)
        {
            Error = exception.Message;
        }
    }
}
