namespace LanguageReader.Shared.Features.Reading;

public sealed record GetReadingProgressRequest(
    Guid ReadingItemId,
    string Username);

