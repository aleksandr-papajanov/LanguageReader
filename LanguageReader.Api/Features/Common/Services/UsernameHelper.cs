using LanguageReader.Infrastructure.Exceptions;

namespace LanguageReader.Api.Features.Common.Services;

internal static class UsernameHelper
{
    public static string Normalize(string? username)
    {
        return username?.Trim().ToLowerInvariant() ?? string.Empty;
    }

    public static string Require(string? username)
    {
        var normalizedUsername = Normalize(username);
        if (string.IsNullOrWhiteSpace(normalizedUsername))
        {
            throw new ValidationException("Username is required.");
        }

        return normalizedUsername;
    }
}
