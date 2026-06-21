namespace LanguageReader.Api.Features.Users;

internal static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder api)
    {
        api.MapCreateSessionEndpoint();
        return api;
    }
}
