using LanguageReader.Api.Features.ReadingItems;
using LanguageReader.Infrastructure.Features.ReadingItemTranslations.Services;
using LanguageReader.Infrastructure.Features.ReadingItems.Services;

namespace LanguageReader.Api.Features.ReadingItemTranslations;

internal sealed class GetReadingItemTranslationsHandler(
    ReadingItemAccessService readingItems,
    ReadingItemTranslationService translations)
{
    public async Task<IReadOnlyList<TranslatedRangeDto>> HandleAsync(GetReadingItemTranslationsRequest request, CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);
        _ = await readingItems.LoadReadableReadOnlyAsync(request.ReadingItemId, normalizedUsername, ct);
        var rows = await translations.LoadForReadingItemAsync(request.ReadingItemId, normalizedUsername, ct);

        return rows.Select(range => range.ToTranslatedRangeDto()).ToList();
    }
}
