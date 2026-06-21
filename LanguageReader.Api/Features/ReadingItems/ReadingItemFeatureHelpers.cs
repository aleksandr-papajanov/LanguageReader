using LanguageReader.Infrastructure.Features.Reading.Entities;
using LanguageReader.Infrastructure.Features.ReadingItems.Entities;
using LanguageReader.Shared.Features.News;

namespace LanguageReader.Api.Features.ReadingItems;

internal static class ReadingItemFeatureHelpers
{
    public const string DefaultNewsSourceKey = NewsSourceKeys.Svt;

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

    public static ReadingItemReadingStatus GetReadingStatus(ReadingProgressEntity? progress)
    {
        if (progress is null || progress.ProgressPercent <= 0)
        {
            return ReadingItemReadingStatus.NotStarted;
        }

        return progress.ProgressPercent >= 100
            ? ReadingItemReadingStatus.Completed
            : ReadingItemReadingStatus.Reading;
    }

    public static bool MatchesReadingState(ReadingItemReadingStatus status, ReadingItemReadingStateFilter filter)
    {
        return filter switch
        {
            ReadingItemReadingStateFilter.NotStarted => status == ReadingItemReadingStatus.NotStarted,
            ReadingItemReadingStateFilter.Reading => status == ReadingItemReadingStatus.Reading,
            ReadingItemReadingStateFilter.Completed => status == ReadingItemReadingStatus.Completed,
            _ => true
        };
    }

    public static string ResolveDiscoverLanguage(string sourceKey)
    {
        return sourceKey switch
        {
            var value when string.Equals(value, NewsSourceKeys.Svt, StringComparison.OrdinalIgnoreCase) => "Swedish",
            var value when string.Equals(value, NewsSourceKeys.Aftonbladet, StringComparison.OrdinalIgnoreCase) => "Swedish",
            var value when string.Equals(value, NewsSourceKeys.SverigesRadio, StringComparison.OrdinalIgnoreCase) => "Swedish",
            _ => "Unknown"
        };
    }

    public static string? ResolveSourceKey(string? sourceName, string? rssFeedUrl, string? originalUrl)
    {
        if (MatchesSource(sourceName, rssFeedUrl, originalUrl, "svt.se", NewsSourceKeys.Svt, "SVT"))
        {
            return NewsSourceKeys.Svt;
        }

        if (MatchesSource(sourceName, rssFeedUrl, originalUrl, "aftonbladet.se", NewsSourceKeys.Aftonbladet, "Aftonbladet"))
        {
            return NewsSourceKeys.Aftonbladet;
        }

        if (MatchesSource(sourceName, rssFeedUrl, originalUrl, "sverigesradio.se", NewsSourceKeys.SverigesRadio, "Sveriges Radio")
            || MatchesSource(sourceName, rssFeedUrl, originalUrl, "api.sr.se", NewsSourceKeys.SverigesRadio, "Sveriges Radio"))
        {
            return NewsSourceKeys.SverigesRadio;
        }

        return null;
    }

    private static bool MatchesSource(
        string? sourceName,
        string? rssFeedUrl,
        string? originalUrl,
        string host,
        string sourceKey,
        string expectedSourceName)
    {
        return string.Equals(expectedSourceName, sourceName, StringComparison.OrdinalIgnoreCase)
            || UrlMatchesHost(rssFeedUrl, host)
            || UrlMatchesHost(originalUrl, host);
    }

    private static bool UrlMatchesHost(string? value, string host)
    {
        if (string.IsNullOrWhiteSpace(value) || !Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return uri.Host.Contains(host, StringComparison.OrdinalIgnoreCase);
    }
}
