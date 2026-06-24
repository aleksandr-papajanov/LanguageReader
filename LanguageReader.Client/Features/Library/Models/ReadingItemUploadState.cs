namespace LanguageReader.Client.Features.Library.Models;

public sealed class ReadingItemUploadState
{
    public static readonly IReadOnlyList<AppTabItem> ImportTabs =
    [
        new("file", "File", "upload_file"),
        new("url", "URL", "link")
    ];

    public string ActiveImportTab { get; private set; } = "file";

    public string Title { get; private set; } = string.Empty;

    public string OriginalLanguage { get; private set; } = "English";

    public string Url { get; private set; } = string.Empty;

    public IBrowserFile? SelectedFile { get; private set; }

    public ReadingItemImportPreviewDto? Preview { get; private set; }

    public string? PreviewError { get; private set; }

    public string? Error { get; private set; }

    public bool IsBusy { get; private set; }

    public bool IsPreviewing { get; private set; }

    public bool IsPreviewOpen { get; private set; }

    public bool HasSelectedFile => SelectedFile is not null;

    public bool HasUrl => Uri.TryCreate(Url.Trim(), UriKind.Absolute, out var uri)
        && uri.Scheme is "http" or "https";

    public void SelectImportTab(string value)
    {
        ActiveImportTab = string.Equals(value, "url", StringComparison.OrdinalIgnoreCase)
            ? "url"
            : "file";
        Error = null;
    }

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
        OriginalLanguage = SupportedLanguages.Normalize(value);
    }

    public void OnUrlChanged(string value)
    {
        Url = value;
    }

    public void ClosePreview()
    {
        IsPreviewOpen = false;
        PreviewError = null;
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

    public async Task PreviewUrlAsync(
        ReadingItemImportsApiClient api,
        CancellationToken cancellationToken = default)
    {
        if (!HasUrl)
        {
            Error = "Enter a valid article URL.";
            return;
        }

        IsPreviewing = true;
        Error = null;
        PreviewError = null;
        Preview = null;
        IsPreviewOpen = true;

        try
        {
            Preview = await api.PreviewUrlImportAsync(
                new PreviewReadingItemUrlImportRequest(Url.Trim(), OriginalLanguage),
                cancellationToken);

            if (string.IsNullOrWhiteSpace(Title))
            {
                Title = Preview.Title;
            }

        }
        catch (Exception exception)
        {
            Preview = null;
            PreviewError = exception.Message;
            IsPreviewOpen = true;
        }
        finally
        {
            IsPreviewing = false;
        }
    }

    public async Task ConfirmUrlImportAsync(
        ReadingItemImportsApiClient api,
        NavigationManager navigation,
        string username,
        CancellationToken cancellationToken = default)
    {
        if (!HasUrl)
        {
            Error = "Enter a valid article URL.";
            return;
        }

        IsBusy = true;
        Error = null;

        try
        {
            await api.ImportUrlAsync(
                new ImportReadingItemUrlClientRequest(
                    username,
                    Url.Trim(),
                    string.IsNullOrWhiteSpace(Title) ? Preview?.Title ?? string.Empty : Title,
                    OriginalLanguage),
                cancellationToken);

            IsPreviewOpen = false;
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
