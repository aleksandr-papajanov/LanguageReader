using LanguageReader.Infrastructure.Ai.Workflows;
using LanguageReader.Infrastructure.Features.ReadingItems.Services;
using LanguageReader.Infrastructure.Features.Settings.Services;
using LanguageReader.Infrastructure.Features.Translation.Workflows;

namespace LanguageReader.Api.Features.Translation;

internal sealed class TranslateSelectionHandler(
    ReadingItemAccessService readingItems,
    UserSettingsService userSettings,
    WorkflowRunner workflowRunner)
{
    public async Task<TranslationResultDto> HandleAsync(TranslateRequest request, CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);
        var settings = await userSettings.GetOrCreateAsync(normalizedUsername, ct);
        var targetLanguage = string.IsNullOrWhiteSpace(request.TargetLanguage)
            ? settings.NativeLanguage ?? string.Empty
            : request.TargetLanguage;
        var sourceLanguage = string.IsNullOrWhiteSpace(request.SourceLanguage)
            ? await readingItems.ResolveOriginalLanguageAsync(request.ReadingItemId, ct) ?? string.Empty
            : request.SourceLanguage;

        var normalizedRequest = request with
        {
            Username = normalizedUsername,
            TargetLanguage = targetLanguage,
            SourceLanguage = sourceLanguage
        };

        return await workflowRunner.RunAsync<TranslateSelectionWorkflow, TranslateRequest, TranslationResultDto>(
            normalizedRequest,
            ct);
    }
}
