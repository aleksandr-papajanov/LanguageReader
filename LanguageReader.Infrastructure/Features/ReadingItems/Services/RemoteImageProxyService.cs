using LanguageReader.Infrastructure.Exceptions;

namespace LanguageReader.Infrastructure.Features.ReadingItems.Services;

public sealed class RemoteImageProxyService(HttpClient httpClient)
{
    private const string BrowserUserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/137.0.0.0 Safari/537.36";

    public async Task<RemoteImageProxyFile> LoadAsync(string url, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            throw new ValidationException("Image URL must be an absolute HTTP or HTTPS URL.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.UserAgent.ParseAdd(BrowserUserAgent);
        request.Headers.Accept.ParseAdd("image/avif,image/webp,image/apng,image/svg+xml,image/*,*/*;q=0.8");

        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new NotFoundException("Remote image could not be loaded.");
        }

        var contentType = response.Content.Headers.ContentType?.MediaType;
        if (string.IsNullOrWhiteSpace(contentType)
            || !contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException("Remote resource is not an image.");
        }

        await using var remoteStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var stream = new MemoryStream();
        await remoteStream.CopyToAsync(stream, cancellationToken);
        stream.Position = 0;

        return new RemoteImageProxyFile(stream, contentType);
    }
}

public sealed record RemoteImageProxyFile(Stream Stream, string ContentType);
