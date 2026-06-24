namespace LanguageReader.Api.Features.Maintenance;

internal static class ResetImportedReadingContentEndpoint
{
    public static IEndpointRouteBuilder MapResetImportedReadingContentEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapPost("/reading-items/maintenance/reset-imported-content", async (
            [AsParameters] ResetImportedReadingContentRequest request,
            ResetImportedReadingContentHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);
            return Results.Ok(result);
        });

        return api;
    }
}
