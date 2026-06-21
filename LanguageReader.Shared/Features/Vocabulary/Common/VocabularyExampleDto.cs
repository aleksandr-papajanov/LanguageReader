namespace LanguageReader.Shared.Features.Vocabulary;

public sealed record VocabularyExampleDto(
    Guid Id,
    string Text,
    string? Translation,
    bool IsFromBook,
    DateTimeOffset CreatedAtUtc,
    Guid? ReadingItemId,
    string? ReadingItemTitle,
    ReadingPositionDto? Position);
