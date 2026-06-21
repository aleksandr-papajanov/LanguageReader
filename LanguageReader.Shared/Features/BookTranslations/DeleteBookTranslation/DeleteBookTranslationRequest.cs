namespace LanguageReader.Shared.Features.BookTranslations;
public sealed record DeleteBookTranslationRequest(
    Guid ReadingItemId,
    Guid TranslationId,
    string Username);

