namespace LanguageReader.Shared.Features.BookTranslations;

public sealed record GetBookTranslationsRequest(
    Guid ReadingItemId,
    string Username);

