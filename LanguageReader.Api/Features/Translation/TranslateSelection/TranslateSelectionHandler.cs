using LanguageReader.Infrastructure.Ai.Workflows;
using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Features.Translation.Workflows;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.Translation;

internal sealed class TranslateSelectionHandler(
    ApplicationDbContext dbContext,
    UserSettingsAccessor userSettingsAccessor,
    WorkflowRunner workflowRunner)
{
    public async Task<TranslationResultDto> HandleAsync(TranslateRequest request, CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);
        var settings = await userSettingsAccessor.GetOrCreateAsync(normalizedUsername, ct);
        var targetLanguage = string.IsNullOrWhiteSpace(request.TargetLanguage)
            ? settings.NativeLanguage ?? string.Empty
            : request.TargetLanguage;
        var sourceLanguage = string.IsNullOrWhiteSpace(request.SourceLanguage)
            ? await ResolveSourceLanguageAsync(request.ReadingItemId, ct) ?? string.Empty
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
