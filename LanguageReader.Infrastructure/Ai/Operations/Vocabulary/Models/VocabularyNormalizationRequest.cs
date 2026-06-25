namespace LanguageReader.Infrastructure.Ai.Operations.Vocabulary;

public sealed record VocabularyNormalizationRequest(
    string Username,
    string Text,
    string Translation,
    string SourceLanguage,
    string TargetLanguage,
    string? ContextSentence);
