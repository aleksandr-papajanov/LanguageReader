namespace LanguageReader.Infrastructure.Ai.Operations.Vocabulary;

public sealed record VocabularyAutofillRequest(
    string Username,
    string Word,
    string Translation,
    string WordLanguage,
    string TranslationLanguage,
    string? ContextSentence);
