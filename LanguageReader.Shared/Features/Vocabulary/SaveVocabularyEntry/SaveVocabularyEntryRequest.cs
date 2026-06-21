namespace LanguageReader.Shared.Features.Vocabulary;

public sealed record SaveVocabularyEntryRequest(
    string Username,
    string Word,
    string? Translation,
    string? SourceLanguage,
    string TargetLanguage,
    Guid ReadingItemId,
    ReadingPositionDto Position,
    string? ContextSentence,
    bool IsVisibleInVocabulary,
    SelectionKind SelectionKind);
