namespace LanguageReader.Infrastructure.Features.Books.Parsing.Models;

public sealed record ParsedBook(
    string? Title,
    IReadOnlyList<ParsedBookBlock> Blocks,
    IReadOnlyDictionary<string, ParsedBookImage> Images);