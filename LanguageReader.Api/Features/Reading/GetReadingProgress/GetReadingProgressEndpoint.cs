namespace LanguageReader.Api.Features.Reading;

internal static class GetReadingProgressEndpoint
{
    public static IEndpointRouteBuilder MapGetReadingProgressEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapGet("/reading-items/{readingItemId:guid}/progress", async (
            [AsParameters] GetReadingProgressRequest request,
            GetReadingProgressHandler handler,
            CancellationToken ct) => Results.Ok(await handler.HandleAsync(request, ct)))
        .WithName("GetReadingProgress")
        .WithOpenApi();

        return api;
    }
}
