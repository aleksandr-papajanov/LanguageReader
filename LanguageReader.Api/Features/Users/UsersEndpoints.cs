namespace LanguageReader.Api.Features.Users;

internal static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder api)
    {
        api.MapCreateSessionEndpoint();
        api.MapRegisterUserEndpoint();
        return api;
    }
}
