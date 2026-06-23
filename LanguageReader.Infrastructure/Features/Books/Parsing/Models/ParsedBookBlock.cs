using LanguageReader.Shared.Features.Books;

namespace LanguageReader.Infrastructure.Features.Books.Parsing.Models;

public sealed record ParsedBookBlock(
    BookBlockType Type,
    string? Text,
    string? ImageId = null);
