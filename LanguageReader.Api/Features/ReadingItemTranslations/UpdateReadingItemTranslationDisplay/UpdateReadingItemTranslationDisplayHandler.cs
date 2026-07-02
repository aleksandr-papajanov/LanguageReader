using LanguageReader.Infrastructure.Features.ReadingItemTranslations.Services;

namespace LanguageReader.Api.Features.ReadingItemTranslations;

internal sealed class UpdateReadingItemTranslationDisplayHandler(ReadingItemTranslationService translations)
{
    public async Task<TranslatedRangeDto> HandleAsync(
        UpdateTranslatedRangeDisplayRequest request,
        CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);
        var range = await translations.UpdateDisplayAsync(
            request.ReadingItemId,
            request.TranslationId,
            normalizedUsername,
            request.ShowOriginal,
            ct);

        return range.ToTranslatedRangeDto();
    }
}
