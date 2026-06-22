using LanguageReader.Infrastructure.Agents.Json.Operations;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Common.Language;
using LanguageReader.Infrastructure.Features.Vocabulary.Models.Enrichment;
using LanguageReader.Infrastructure.Features.Vocabulary.Operations;
using LanguageReader.Shared.Features.Settings;

namespace LanguageReader.Infrastructure.Features.Vocabulary.Services.Enrichment;

/// <summary>
/// Vocabulary enrichment backend backed by direct provider JSON responses.
/// </summary>
public sealed class AiVocabularyEnrichmentService(
    IAiJsonOperationRunner operationRunner,
    IVocabularyNormalizationRuleProvider normalizationRuleProvider) : IVocabularyEnrichmentBackend
{
    public AiServiceMode Mode => AiServiceMode.Agent;

    public async Task<VocabularyNormalizationResult> NormalizeAsync(
        VocabularyNormalizationRequest request,
        CancellationToken cancellationToken = default)
    {
        var rules = normalizationRuleProvider.GetRules(request.SourceLanguage);
        var result = await operationRunner.RunAsync(new VocabularyCandidateAiJsonOperation(request, rules), cancellationToken);
        var dictionaryForm = result.Payload.IsLexicalUnit
            ? NormalizeRequiredText("dictionaryForm", result.Payload.DictionaryForm)
            : string.Empty;

        return new VocabularyNormalizationResult(result.Payload.IsLexicalUnit, dictionaryForm, result.Usage);
    }

    public async Task<VocabularyAutofillResult> AutofillAsync(
        VocabularyAutofillRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await operationRunner.RunAsync(new VocabularyAutofillAiJsonOperation(request), cancellationToken);
        var payload = result.Payload;

        if (string.IsNullOrWhiteSpace(payload.PrimaryTranslation))
        {
            throw new InfrastructureException("Vocabulary autofill must include primaryTranslation.");
        }

        if (string.IsNullOrWhiteSpace(payload.Description))
        {
            throw new InfrastructureException("Vocabulary autofill must include description.");
        }

        var primaryTranslation = NormalizeRequiredText("primaryTranslation", payload.PrimaryTranslation);
        var description = NormalizeRequiredText("description", payload.Description);
        var notes = NormalizeOptionalText(payload.Notes);

        var relatedWords = NormalizeList(payload.Synonyms)
            .Select(item => new VocabularyRelatedWordSeed(item, RelatedWordType.Synonym))
            .Concat(NormalizeList(payload.Antonyms)
                .Select(item => new VocabularyRelatedWordSeed(item, RelatedWordType.Antonym)))
            .Concat(NormalizeList(payload.Related)
                .Select(item => new VocabularyRelatedWordSeed(item, RelatedWordType.Related)))
            .ToList();

        return new VocabularyAutofillResult(
            primaryTranslation,
            description,
            Math.Clamp(payload.FrequencyScore, 0, 100),
            NormalizeList(payload.AlternativeTranslations).ToList(),
            relatedWords,
            result.Usage,
            string.IsNullOrWhiteSpace(payload.PartOfSpeech) ? null : payload.PartOfSpeech.Trim(),
            notes);
    }

    public async Task<VocabularyGeneratedExampleResult> GenerateExampleAsync(
        VocabularyExampleGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await operationRunner.RunAsync(new VocabularyExampleAiJsonOperation(request), cancellationToken);
        var payload = result.Payload;

        if (string.IsNullOrWhiteSpace(payload.Text))
        {
            throw new InfrastructureException("Vocabulary example must include text.");
        }

        if (string.IsNullOrWhiteSpace(payload.Translation))
        {
            throw new InfrastructureException("Vocabulary example must include translation.");
        }

        var exampleText = NormalizeRequiredText("text", payload.Text);
        var exampleTranslation = NormalizeRequiredText("translation", payload.Translation);

        return new VocabularyGeneratedExampleResult(
            exampleText,
            exampleTranslation,
            result.Usage);
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
