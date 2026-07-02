using LanguageReader.Infrastructure.Features.ReadingItems.Services;

namespace LanguageReader.Api.Features.ReadingItems;

internal sealed class GetRemoteImageProxyHandler(RemoteImageProxyService remoteImages)
{
    public async Task<RemoteImageProxyResult> HandleAsync(string url, CancellationToken ct)
    {
        var image = await remoteImages.LoadAsync(url, ct);
        return new RemoteImageProxyResult(image.Stream, image.ContentType);
    }
}

internal sealed record RemoteImageProxyResult(Stream Stream, string ContentType);
