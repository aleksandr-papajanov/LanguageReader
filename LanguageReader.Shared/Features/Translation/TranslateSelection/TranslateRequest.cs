namespace LanguageReader.Shared.Features.Translation;

public sealed record TranslateRequest(
    string Username,
    string TargetLanguage,
    string SourceLanguage,
    SelectionKind SelectionKind,
    string SourceText,
    string? OriginalText,
    Guid? ReadingItemId,
    ReadingPositionDto Position);
