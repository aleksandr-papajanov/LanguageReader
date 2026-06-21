using LanguageReader.Infrastructure.Features.Vocabulary.Models.Enrichment;

namespace LanguageReader.Infrastructure.Features.Vocabulary.Services.Enrichment;

/// <summary>
/// Generates mock vocabulary enrichment content until AI-backed enrichment is implemented.
/// </summary>
public interface IVocabularyEnrichmentService
{
    Task<VocabularyNormalizationResult> NormalizeAsync(
        VocabularyNormalizationRequest request,
        CancellationToken cancellationToken = default);

    Task<VocabularyAutofillResult> AutofillAsync(
        VocabularyAutofillRequest request,
        CancellationToken cancellationToken = default);

    Task<VocabularyGeneratedExampleResult> GenerateExampleAsync(
        VocabularyExampleGenerationRequest request,
        CancellationToken cancellationToken = default);
}
