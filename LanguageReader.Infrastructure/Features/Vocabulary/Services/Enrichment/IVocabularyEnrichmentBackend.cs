using LanguageReader.Infrastructure.Features.Vocabulary.Models.Enrichment;
using LanguageReader.Shared.Features.Settings;

namespace LanguageReader.Infrastructure.Features.Vocabulary.Services.Enrichment;

public interface IVocabularyEnrichmentBackend
{
    AiServiceMode Mode { get; }

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
