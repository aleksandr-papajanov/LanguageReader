using LanguageReader.Infrastructure.Ai.Execution;
using LanguageReader.Infrastructure.Ai.Operations.Vocabulary;
using LanguageReader.Infrastructure.Ai.Workflows;
using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Common.Language;
using LanguageReader.Infrastructure.Features.Vocabulary.Entities;
using LanguageReader.Infrastructure.Features.Vocabulary.Models.Enrichment;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Infrastructure.Features.Vocabulary.Workflows;

public sealed class SaveVocabularyEntryWorkflow(
    ApplicationDbContext dbContext,
    IVocabularyNormalizationRuleProvider normalizationRuleProvider)
    : IWorkflow<SaveVocabularyEntryWorkflowRequest, SaveVocabularyEntryWorkflowResult>
{
    public async Task<SaveVocabularyEntryWorkflowResult> RunAsync(
        SaveVocabularyEntryWorkflowRequest request,
        WorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var wordLanguage = string.IsNullOrWhiteSpace(request.SourceLanguage)
            ? request.TargetLanguage
            : request.SourceLanguage;
        var shouldResolveCandidate = request.SelectionKind is SelectionKind.Word or SelectionKind.Unknown;
        var normalization = shouldResolveCandidate
            ? await NormalizeAsync(request, wordLanguage, context, cancellationToken)
            : null;
        var kind = request.SelectionKind == SelectionKind.Word || normalization?.IsLexicalUnit == true
            ? SavedTextKind.LexicalUnit
            : FromSelectionKind(request.SelectionKind);
        var dictionaryForm = kind == SavedTextKind.LexicalUnit && !string.IsNullOrWhiteSpace(normalization?.DictionaryForm)
            ? normalization.DictionaryForm.Trim()
            : null;
        var canonicalText = dictionaryForm ?? request.Text.Trim();

        if (kind != SavedTextKind.LexicalUnit)
        {
            return new SaveVocabularyEntryWorkflowResult(
                kind,
                canonicalText,
                dictionaryForm,
                normalization,
                ExistingEntry: null,
                Autofill: null);
        }

        var existingEntry = await FindExistingLexicalEntryAsync(
            request.Username,
            wordLanguage,
            request.TargetLanguage,
            canonicalText,
            cancellationToken);

        if (existingEntry is not null)
        {
            return new SaveVocabularyEntryWorkflowResult(
                kind,
                canonicalText,
                dictionaryForm,
                normalization,
                existingEntry,
                Autofill: null);
        }

        var autofill = await AutofillAsync(
            request,
            canonicalText,
            wordLanguage,
            context,
            cancellationToken);

        return new SaveVocabularyEntryWorkflowResult(
            kind,
            canonicalText,
            dictionaryForm,
            normalization,
            ExistingEntry: null,
            autofill);
    }

    private async Task<VocabularyNormalizationResult> NormalizeAsync(
        SaveVocabularyEntryWorkflowRequest request,
        string wordLanguage,
        WorkflowContext context,
        CancellationToken cancellationToken)
    {
        var classification = await context.ExecuteAsync(
            new LexicalUnitClassificationOperation(
                new VocabularyNormalizationRequest(
                    request.Username,
                    request.Text,
                    request.Translation,
                    wordLanguage,
                    request.TargetLanguage,
                    request.ContextSentence)),
            cancellationToken);

        if (!classification.Payload.IsLexicalUnit)
        {
            return new VocabularyNormalizationResult(false, string.Empty, null, classification.Usage);
        }

        var rules = normalizationRuleProvider.GetRules(request.SourceLanguage ?? request.TargetLanguage);
        var normalization = await context.ExecuteAsync(
            new VocabularyFormNormalizationOperation(
                new VocabularyNormalizationRequest(
                    request.Username,
                    request.Text,
                    request.Translation,
                    wordLanguage,
                    request.TargetLanguage,
                    request.ContextSentence),
                rules),
            cancellationToken);

        return new VocabularyNormalizationResult(
            true,
            NormalizeRequiredText("dictionaryForm", normalization.Payload.DictionaryForm),
            NormalizeRequiredText("partOfSpeech", normalization.Payload.PartOfSpeech),
            normalization.Usage);
    }

    private static async Task<VocabularyAutofillResult> AutofillAsync(
        SaveVocabularyEntryWorkflowRequest request,
        string canonicalText,
        string wordLanguage,
        WorkflowContext context,
        CancellationToken cancellationToken)
    {
        var result = await context.ExecuteAsync(
            new VocabularyDetailsOperation(
                new VocabularyAutofillRequest(
                    request.Username,
                    canonicalText,
                    request.Translation,
                    wordLanguage,
                    request.TargetLanguage,
                    request.ContextSentence)),
            cancellationToken);

        return BuildAutofillResult(result);
    }

    private async Task<VocabularyEntryEntity?> FindExistingLexicalEntryAsync(
        string username,
        string wordLanguage,
        string targetLanguage,
        string canonicalText,
        CancellationToken cancellationToken)
    {
        var normalizedWordLanguage = wordLanguage.Trim().ToLowerInvariant();
        var normalizedTargetLanguage = targetLanguage.Trim().ToLowerInvariant();
        var normalizedCanonicalText = canonicalText.Trim().ToLowerInvariant();

        return await dbContext.VocabularyEntries
            .Include(item => item.ReadingItem)
            .Include(item => item.WordDetails)
            .Include(item => item.RelatedWords)
            .Include(item => item.AiOperations)
            .Include(item => item.Examples)
                .ThenInclude(example => example.ReadingItem)
            .OrderByDescending(item => item.CreatedAtUtc)
            .FirstOrDefaultAsync(existing =>
                existing.Username == username
                && existing.Kind == SavedTextKind.LexicalUnit
                && existing.TargetLanguage.ToLower() == normalizedTargetLanguage
                && (existing.SourceLanguage == null
                    || existing.SourceLanguage == string.Empty
                    || existing.SourceLanguage.ToLower() == normalizedWordLanguage)
                && ((existing.WordDetails != null
                        && existing.WordDetails.DictionaryForm != null
                        && existing.WordDetails.DictionaryForm.ToLower() == normalizedCanonicalText)
                    || existing.Word.ToLower() == normalizedCanonicalText),
                cancellationToken);
    }

    private static SavedTextKind FromSelectionKind(SelectionKind selectionKind)
    {
        return selectionKind == SelectionKind.Word
            ? SavedTextKind.LexicalUnit
            : SavedTextKind.Text;
    }

    public static VocabularyAutofillResult BuildAutofillResult(
        AiOperationExecutionResult<VocabularyDetailsOperation.Payload> result)
    {
        var payload = result.Payload;

        if (string.IsNullOrWhiteSpace(payload.PrimaryTranslation))
        {
            throw new InfrastructureException("Vocabulary autofill must include primaryTranslation.");
        }

        if (string.IsNullOrWhiteSpace(payload.Description))
        {
            throw new InfrastructureException("Vocabulary autofill must include description.");
        }

        var relatedWords = NormalizeList(payload.Synonyms)
            .Select(item => new VocabularyRelatedWordSeed(item, RelatedWordType.Synonym))
            .Concat(NormalizeList(payload.Antonyms)
                .Select(item => new VocabularyRelatedWordSeed(item, RelatedWordType.Antonym)))
            .Concat(NormalizeList(payload.Related)
                .Select(item => new VocabularyRelatedWordSeed(item, RelatedWordType.Related)))
            .ToList();

        return new VocabularyAutofillResult(
            NormalizeRequiredText("primaryTranslation", payload.PrimaryTranslation),
            NormalizeRequiredText("description", payload.Description),
            Math.Clamp(payload.FrequencyScore, 0, 100),
            NormalizeList(payload.AlternativeTranslations).ToList(),
            relatedWords,
            result.Usage,
            PartOfSpeech: null,
            NormalizeOptionalText(payload.Notes));
    }

    private static IReadOnlyList<string> NormalizeList(IReadOnlyList<string>? items)
    {
        return items?
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray()
            ?? [];
    }

    private static string NormalizeRequiredText(string fieldName, string value)
    {
        var normalized = value.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InfrastructureException($"Vocabulary payload must include {fieldName}.");
        }

        return normalized;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

}

public sealed record SaveVocabularyEntryWorkflowRequest(
    string Username,
    string Text,
    string Translation,
    string? SourceLanguage,
    string TargetLanguage,
    SelectionKind SelectionKind,
    string? ContextSentence);

public sealed record SaveVocabularyEntryWorkflowResult(
    SavedTextKind Kind,
    string CanonicalText,
    string? DictionaryForm,
    VocabularyNormalizationResult? Normalization,
    VocabularyEntryEntity? ExistingEntry,
    VocabularyAutofillResult? Autofill);
