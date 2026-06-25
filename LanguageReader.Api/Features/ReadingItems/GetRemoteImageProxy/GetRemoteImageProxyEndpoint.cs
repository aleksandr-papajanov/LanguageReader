using Microsoft.AspNetCore.Mvc;

namespace LanguageReader.Api.Features.ReadingItems;

internal static class GetRemoteImageProxyEndpoint
{
    public static IEndpointRouteBuilder MapGetRemoteImageProxyEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapGet("/reading-items/image-proxy", async (
            [FromQuery] string url,
            GetRemoteImageProxyHandler handler,
            CancellationToken ct) =>
        {
            var image = await handler.HandleAsync(url, ct);
            return Results.File(image.Stream, image.ContentType);
        });

        return api;
    }
}
