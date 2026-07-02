using LanguageReader.Infrastructure.Ai.Execution;
using LanguageReader.Infrastructure.Ai.Operations.Vocabulary;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Vocabulary.Services;
using LanguageReader.Infrastructure.Features.Vocabulary.Workflows;

namespace LanguageReader.Api.Features.Vocabulary;

internal sealed class AutofillVocabularyEntryHandler(
    IAiExecutor aiExecutor,
    VocabularyEntryGraphService vocabularyEntries,
    VocabularyAutofillApplicator autofillApplicator)
{
    public async Task<VocabularyEntryDto> HandleAsync(AutofillVocabularyEntryRequest request, CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);
        var entry = await vocabularyEntries.LoadOwnedAsync(request.VocabularyId, normalizedUsername, ct);

        if (entry.Kind != SavedTextKind.LexicalUnit)
        {
            throw new ValidationException("Autofill is only available for saved words.");
        }

        var result = await aiExecutor.ExecuteAsync(
            new VocabularyDetailsOperation(
                new VocabularyAutofillRequest(
                    entry.Username,
                    entry.Word,
                    entry.Translation,
                    string.IsNullOrWhiteSpace(entry.SourceLanguage) ? entry.TargetLanguage : entry.SourceLanguage,
                    entry.TargetLanguage,
                    entry.Examples.FirstOrDefault(example => example.IsFromReadingItem)?.Text)),
            ct);

        await autofillApplicator.ApplyAndSaveAsync(
            entry,
            SaveVocabularyEntryWorkflow.BuildAutofillResult(result),
            ct);

        return entry.ToVocabularyEntryDto();
    }
}
