using LanguageReader.Infrastructure.Features.Books.Entities;

namespace LanguageReader.Api.Features.Books;

internal static class BookFeatureHelpers
{
    public static string NormalizeUsername(string? username)
    {
        return UsernameHelper.Normalize(username);
    }

    public static string RequireUsername(string? username)
    {
        return UsernameHelper.Require(username);
    }

    public static string NormalizeLanguage(string? language)
    {
        var normalized = language?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? "Unknown" : normalized;
    }

    public static bool CanRead(BookEntity book, string username)
    {
        return book.IsPublic || book.OwnerUsername == username;
    }
}
