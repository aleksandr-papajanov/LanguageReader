namespace LanguageReader.Shared.Features.ReadingItemTranslations;
public sealed record DeleteReadingItemTranslationRequest(
    Guid ReadingItemId,
    Guid TranslationId,
    string Username);

