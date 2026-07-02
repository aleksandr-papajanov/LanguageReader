using LanguageReader.Infrastructure.Features.ReadingItemTranslations.Services;

namespace LanguageReader.Api.Features.ReadingItemTranslations;

internal sealed class DeleteReadingItemTranslationHandler(ReadingItemTranslationService translations)
{
    public async Task HandleAsync(DeleteReadingItemTranslationRequest request, CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);
        await translations.DeleteAsync(
            request.ReadingItemId,
            request.TranslationId,
            normalizedUsername,
            ct);
    }
}

