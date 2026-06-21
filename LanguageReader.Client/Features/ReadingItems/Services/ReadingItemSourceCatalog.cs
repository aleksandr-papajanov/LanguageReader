using LanguageReader.Client.Features.Common.Components;
using LanguageReader.Client.Features.ReadingItems.Models;
using LanguageReader.Shared.Features.News;

namespace LanguageReader.Client.Features.ReadingItems.Services;

public static class ReadingItemSourceCatalog
{
    private static readonly IReadOnlyList<ReadingItemSourceInfo> Sources =
    [
        new(NewsSourceKeys.Svt, "SVT", "/assets/svt.svg"),
        new(NewsSourceKeys.Aftonbladet, "Aftonbladet", "/assets/aftonbladet.svg"),
        new(NewsSourceKeys.SverigesRadio, "Sveriges Radio", "/assets/sveries-radio.svg")
    ];

    public static IReadOnlyList<ReadingItemSourceInfo> NewsSources => Sources;

    public static ReadingItemSourceInfo? Find(string? sourceKey)
    {
        if (string.IsNullOrWhiteSpace(sourceKey))
        {
            return null;
        }

        return Sources.FirstOrDefault(source =>
            string.Equals(source.SourceKey, sourceKey, StringComparison.OrdinalIgnoreCase));
    }

    public static IReadOnlyList<AppTabItem> ToTabs()
    {
        return Sources
            .Select(source => new AppTabItem(
                source.SourceKey,
                source.DisplayName,
                // ImageSrc: source.LogoAssetPath
                null))
            .ToList();
    }
}
