namespace LanguageReader.Api.Features.Stats;

internal static class StatsEndpoints
{
    public static IEndpointRouteBuilder MapStatsEndpoints(this IEndpointRouteBuilder api)
    {
        api.MapGet("/stats/ai-operations", async (
            [AsParameters] GetAiOperationsStatsRequest request,
            GetAiOperationsStatsHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);
            return Results.Ok(result);
        });

        return api;
    }
}
