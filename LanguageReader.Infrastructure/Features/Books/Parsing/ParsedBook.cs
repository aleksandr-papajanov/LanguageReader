namespace LanguageReader.Infrastructure.Features.Books.Parsing;

/// <summary>
/// Parsed text content for a book file.
/// </summary>
public sealed record ParsedBook(string? Title, IReadOnlyList<string> Pages);

