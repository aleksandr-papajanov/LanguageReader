namespace LanguageReader.Shared.Features.Common;

public sealed record ReadingPositionDto(
    Guid ReadingItemId,
    int ParagraphIndex,
    int CharacterOffset);
