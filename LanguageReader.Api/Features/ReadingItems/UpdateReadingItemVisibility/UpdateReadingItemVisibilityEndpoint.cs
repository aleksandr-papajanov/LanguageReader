namespace LanguageReader.Api.Features.ReadingItems;

internal static class UpdateReadingItemVisibilityEndpoint
{
    public static IEndpointRouteBuilder MapUpdateReadingItemVisibilityEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapPut("/reading-items/{ReadingItemId:guid}/visibility", async (
            [AsParameters] UpdateReadingItemVisibilityRequestRoute route,
            UpdateReadingItemVisibilityRequestBody body,
            UpdateReadingItemVisibilityHandler handler,
            CancellationToken ct) =>
        {
            var request = new UpdateReadingItemVisibilityRequest(
                route.ReadingItemId,
                body.Username,
                body.IsPublic);

            await handler.HandleAsync(request, ct);
            return Results.NoContent();
        });

        return api;
    }
}
