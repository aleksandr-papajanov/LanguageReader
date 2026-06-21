namespace LanguageReader.Api.Features.Users;

internal static class SessionMappingExtensions
{
    public static SessionDto ToSessionDto(this string username)
    {
        return new SessionDto(username);
    }
}
