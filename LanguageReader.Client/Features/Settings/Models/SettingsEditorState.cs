namespace LanguageReader.Client.Features.Settings.Models;

public sealed class SettingsEditorState
{
    public static readonly IReadOnlyList<AppSelectOption<AiServiceMode>> AiModeOptions =
    [
        new(AiServiceMode.Fake, "Fake"),
        new(AiServiceMode.Agent, "Agent")
    ];

    public string? NativeLanguage { get; private set; }

    public AiServiceMode AiServiceMode { get; private set; } = AiServiceMode.Fake;

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

            NativeLanguage = settings.NativeLanguage;
            AiServiceMode = settings.AiServiceMode;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void OnNativeLanguageChanged(string value)
    {
        NativeLanguage = value;
    }

    public void OnAiServiceModeChanged(AiServiceMode value)
    {
        AiServiceMode = value;
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
                new UpdateUserSettingsRequest(username, NativeLanguage, AiServiceMode),
                cancellationToken);

            Message = "Settings saved.";
        }
        catch (Exception exception)
        {
            Error = exception.Message;
        }
    }
}
