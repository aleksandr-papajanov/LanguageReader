namespace LanguageReader.Api.Features.Users;

internal static class RegisterUserEndpoint
{
    public static IEndpointRouteBuilder MapRegisterUserEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapPost("/users/register", async (
            RegisterUserRequest request,
            RegisterUserHandler handler,
            CancellationToken ct) => Results.Ok(await handler.HandleAsync(request, ct)))
        .WithName("RegisterUser")
        .WithOpenApi();

        return api;
    }
}
