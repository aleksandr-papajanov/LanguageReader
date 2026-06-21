namespace LanguageReader.Shared.Features.Reading;

public sealed record ReadingProgressDto(
    Guid ReadingItemId,
    string Username,
    double ProgressPercent,
    DateTimeOffset LastOpenedAtUtc,
    ReadingPositionDto Position);

