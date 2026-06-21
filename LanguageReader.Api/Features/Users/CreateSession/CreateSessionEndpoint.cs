using LanguageReader.Shared.Features.Users;

namespace LanguageReader.Api.Features.Users;

internal static class CreateSessionEndpoint
{
    public static IEndpointRouteBuilder MapCreateSessionEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapPost("/session", (
            LoginRequest request,
            CreateSessionHandler handler) => Results.Ok(handler.Handle(request)))
        .WithName("CreateSession")
        .WithOpenApi();

        return api;
    }
}


