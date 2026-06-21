namespace LanguageReader.Infrastructure.Features.ReadingItems.Models;

public sealed record StoredArticleDocument(
    string Title,
    string OriginalLanguage,
    IReadOnlyList<string> Paragraphs);
