namespace LanguageReader.Infrastructure.Features.Vocabulary.Models.Enrichment;

public sealed record VocabularyNormalizationResult(
    bool IsLexicalUnit,
    string DictionaryForm,
    string? PartOfSpeech,
    AiOperationUsageDto Usage);
