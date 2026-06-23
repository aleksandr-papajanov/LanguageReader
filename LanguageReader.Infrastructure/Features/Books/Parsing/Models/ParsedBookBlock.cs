namespace LanguageReader.Infrastructure.Features.Books.Parsing.Models;

public sealed record ParsedBookBlock(
    ReadingContentBlockType Type,
    string? Text,
    string? ImageId = null);
