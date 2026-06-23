namespace LanguageReader.Client.Features.Library.Models;

public sealed class ReadingItemUploadState
{
    public string Title { get; private set; } = string.Empty;

    public string OriginalLanguage { get; private set; } = "English";

    public IBrowserFile? SelectedFile { get; private set; }

    public string? Error { get; private set; }

    public bool IsBusy { get; private set; }

    public bool HasSelectedFile => SelectedFile is not null;

    public void OnFileSelected(InputFileChangeEventArgs args)
    {
        SelectedFile = args.File;
    }

    public void OnTitleChanged(string value)
    {
        Title = value;
    }

    public void OnOriginalLanguageChanged(string value)
    {
        OriginalLanguage = value;
    }

    public async Task UploadAsync(
        ReadingItemImportsApiClient api,
        NavigationManager navigation,
        string username,
        CancellationToken cancellationToken = default)
    {
        if (SelectedFile is null)
        {
            return;
        }

        IsBusy = true;
        Error = null;

        try
        {
            await api.ImportReadingItemAsync(
                new ImportReadingItemClientRequest(
                    username,
                    Title,
                    OriginalLanguage,
                    SelectedFile),
                cancellationToken);

            navigation.NavigateTo("/?tab=my-books");
        }
        catch (Exception exception)
        {
            Error = exception.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
