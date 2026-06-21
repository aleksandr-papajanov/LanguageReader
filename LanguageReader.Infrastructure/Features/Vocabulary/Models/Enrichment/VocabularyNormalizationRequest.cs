namespace LanguageReader.Infrastructure.Features.Vocabulary.Models.Enrichment;

public sealed record VocabularyNormalizationRequest(
    string Username,
    string Text,
    string Translation,
    string SourceLanguage,
    string TargetLanguage,
    string? ContextSentence);
