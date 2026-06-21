using LanguageReader.Api.Features.Common.Services;
using LanguageReader.Api.Features.Settings;
using LanguageReader.Api.Features.Settings.Services;
using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Features.Translation.Services;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.Translation;

internal sealed class TranslateSelectionHandler(
    ApplicationDbContext dbContext,
    UserSettingsAccessor userSettingsAccessor,
    ITranslationService translationService)
{
    public async Task<TranslationResultDto> HandleAsync(TranslateRequest request, CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);
        var settings = await userSettingsAccessor.GetOrCreateAsync(normalizedUsername, ct);
        var targetLanguage = string.IsNullOrWhiteSpace(request.TargetLanguage)
            ? settings.NativeLanguage ?? string.Empty
            : request.TargetLanguage;
        var sourceLanguage = string.IsNullOrWhiteSpace(request.SourceLanguage)
            ? await ResolveSourceLanguageAsync(request.ReadingItemId, ct)
            : request.SourceLanguage;

        var normalizedRequest = request with
        {
            Username = normalizedUsername,
            TargetLanguage = targetLanguage,
            SourceLanguage = sourceLanguage
        };

        return await translationService.TranslateAsync(normalizedRequest, ct);
    }

    private async Task<string?> ResolveSourceLanguageAsync(Guid? readingItemId, CancellationToken ct)
    {
        if (!readingItemId.HasValue)
        {
            return null;
        }

        return await dbContext.ReadingItems
            .Where(item => item.Id == readingItemId.Value)
            .Select(item => item.OriginalLanguage)
            .FirstOrDefaultAsync(ct);
    }
}
