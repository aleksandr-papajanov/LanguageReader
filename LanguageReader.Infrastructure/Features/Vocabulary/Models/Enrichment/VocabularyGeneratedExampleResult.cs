namespace LanguageReader.Infrastructure.Features.Vocabulary.Models.Enrichment;

public sealed record VocabularyGeneratedExampleResult(
    string Text,
    string? Translation,
    AiOperationUsageDto Usage);
