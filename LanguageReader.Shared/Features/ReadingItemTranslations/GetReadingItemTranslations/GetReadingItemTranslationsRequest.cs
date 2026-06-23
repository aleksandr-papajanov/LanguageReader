namespace LanguageReader.Shared.Features.ReadingItemTranslations;

public sealed record GetReadingItemTranslationsRequest(
    Guid ReadingItemId,
    string Username);

