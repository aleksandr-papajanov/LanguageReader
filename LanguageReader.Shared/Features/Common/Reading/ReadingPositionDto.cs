namespace LanguageReader.Shared.Features.Common;

public sealed record ReadingPositionDto(
    Guid ReadingItemId,
    int BlockIndex,
    int CharacterOffset);
