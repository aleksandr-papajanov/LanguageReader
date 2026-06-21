namespace LanguageReader.Infrastructure.Features.Vocabulary.Models.Enrichment;

public sealed record VocabularyAutofillResult(
    string DictionaryForm,
    string PrimaryTranslation,
    string Description,
    int FrequencyScore,
    IReadOnlyList<string> AlternativeTranslations,
    IReadOnlyList<VocabularyRelatedWordSeed> RelatedWords,
    AiOperationUsageDto Usage,
    string? PartOfSpeech = null,
    string? Notes = null);
