using System.Net.Http.Headers;

namespace LanguageReader.Client.Features.Library.Services;

public sealed record ImportReadingItemClientRequest(
    string Username,
    string Title,
    string OriginalLanguage,
    IBrowserFile File);

public sealed record ImportReadingItemUrlClientRequest(
    string Username,
    string Url,
    string Title,
    string OriginalLanguage);

public sealed class ReadingItemImportsApiClient(ApiClient api)
{
    public async Task<ReadingItemDetailsDto> ImportReadingItemAsync(
        ImportReadingItemClientRequest request,
        CancellationToken cancellationToken = default)
    {
        using var form = new MultipartFormDataContent();

        form.Add(new StringContent(request.Username), "username");
        form.Add(new StringContent(request.Title ?? string.Empty), "title");
        form.Add(new StringContent(request.OriginalLanguage ?? string.Empty), "originalLanguage");

        var fileContent = new StreamContent(
            request.File.OpenReadStream(
                maxAllowedSize: 25 * 1024 * 1024,
                cancellationToken));

        var contentType = string.IsNullOrWhiteSpace(request.File.ContentType)
            ? "application/octet-stream"
            : request.File.ContentType;

        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        form.Add(fileContent, "file", request.File.Name);

        return await api.SendMultipartAsync<ReadingItemDetailsDto>(
            "/api/reading-items/imports/fb2",
            form,
            cancellationToken);
    }

    public Task<ReadingItemImportPreviewDto> PreviewUrlImportAsync(
        PreviewReadingItemUrlImportRequest request,
        CancellationToken cancellationToken = default)
    {
        return api.PostAsync<ReadingItemImportPreviewDto>(
            "/api/reading-items/imports/url/preview",
            request,
            cancellationToken);
    }

    public Task<ReadingItemDetailsDto> ImportUrlAsync(
        ImportReadingItemUrlClientRequest request,
        CancellationToken cancellationToken = default)
    {
        return api.PostAsync<ReadingItemDetailsDto>(
            "/api/reading-items/imports/url",
            new ImportReadingItemFromUrlRequest(
                request.Username,
                request.Url,
                request.Title,
                request.OriginalLanguage),
            cancellationToken);
    }
}
