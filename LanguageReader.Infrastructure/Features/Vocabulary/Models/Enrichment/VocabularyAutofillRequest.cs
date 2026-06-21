namespace LanguageReader.Infrastructure.Features.Vocabulary.Models.Enrichment;

public sealed record VocabularyAutofillRequest(
    string Username,
    string Word,
    string Translation,
    string WordLanguage,
    string TranslationLanguage,
    string? ContextSentence);
