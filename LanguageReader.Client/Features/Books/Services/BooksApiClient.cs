using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Forms;

namespace LanguageReader.Client.Features.Books.Services;

public sealed record UploadBookClientRequest(
    string Username,
    string Title,
    string OriginalLanguage,
    IBrowserFile File);

public sealed class BooksApiClient(ApiClient api)
{
    public async Task<BookDetailsDto> UploadBookAsync(
        UploadBookClientRequest request,
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

        return await api.SendMultipartAsync<BookDetailsDto>(
            "/api/books",
            form,
            cancellationToken);
    }

    public Task<BookDetailsDto> GetBookAsync(
        GetBookRequest request,
        CancellationToken cancellationToken = default)
    {
        return api.GetAsync<BookDetailsDto>(
            "/api/books/{BookId}",
            request,
            cancellationToken);
    }

    public Task<BookContentDto> GetBookContentAsync(
        GetBookContentRequest request,
        CancellationToken cancellationToken = default)
    {
        return api.GetAsync<BookContentDto>(
            "/api/books/{BookId}/content",
            request,
            cancellationToken);
    }

    public Task SetVisibilityAsync(
        UpdateBookVisibilityRequest request,
        CancellationToken cancellationToken = default)
    {
        var route = new UpdateBookVisibilityRequestRoute(request.BookId);
        var body = new UpdateBookVisibilityRequestBody(request.Username, request.IsPublic);

        return api.PutAsync(
            "/api/books/{BookId}/visibility",
            route,
            body,
            cancellationToken);
    }

    public Task DeleteBookAsync(
        DeleteBookRequest request,
        CancellationToken cancellationToken = default)
    {
        return api.DeleteAsync(
            "/api/books/{BookId}",
            request,
            cancellationToken);
    }
}
