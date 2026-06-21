namespace LanguageReader.Shared.Features.Books;

public sealed record BookContentDto(
    Guid Id,
    string Title,
    string Author,
    string OriginalLanguage,
    IReadOnlyList<string> Paragraphs);

