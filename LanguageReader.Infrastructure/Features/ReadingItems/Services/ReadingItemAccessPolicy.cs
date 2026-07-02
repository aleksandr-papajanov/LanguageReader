using LanguageReader.Infrastructure.Features.ReadingItems.Entities;

namespace LanguageReader.Infrastructure.Features.ReadingItems.Services;

public static class ReadingItemAccessPolicy
{
    public static string NormalizeUsername(string? username)
    {
        return string.IsNullOrWhiteSpace(username)
            ? string.Empty
            : username.Trim();
    }

    public static bool CanRead(ReadingItemEntity item, string? username)
    {
        if (item.IsPublic)
        {
            return true;
        }

        var normalizedUsername = NormalizeUsername(username);
        return !string.IsNullOrWhiteSpace(normalizedUsername)
            && string.Equals(item.OwnerUsername, normalizedUsername, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsOwner(ReadingItemEntity item, string username)
    {
        return string.Equals(item.OwnerUsername, username, StringComparison.OrdinalIgnoreCase);
    }
}
