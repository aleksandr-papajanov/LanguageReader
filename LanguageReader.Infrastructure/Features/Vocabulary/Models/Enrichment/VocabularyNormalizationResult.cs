namespace LanguageReader.Infrastructure.Features.Vocabulary.Models.Enrichment;

public sealed record VocabularyNormalizationResult(
    string DictionaryForm,
    AiOperationUsageDto Usage);
