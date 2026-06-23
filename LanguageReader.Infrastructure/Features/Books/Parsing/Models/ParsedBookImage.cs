namespace LanguageReader.Infrastructure.Features.Books.Parsing.Models;

public sealed record ParsedBookImage(
    string Id,
    string ContentType,
    string Base64Content);
