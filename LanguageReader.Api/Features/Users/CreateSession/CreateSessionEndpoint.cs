namespace LanguageReader.Api.Features.Users;

internal static class CreateSessionEndpoint
{
    public static IEndpointRouteBuilder MapCreateSessionEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapPost("/session", async (
            LoginRequest request,
            CreateSessionHandler handler,
            CancellationToken ct) => Results.Ok(await handler.HandleAsync(request, ct)))
        .WithName("CreateSession")
        .WithOpenApi();

        return api;
    }
}

