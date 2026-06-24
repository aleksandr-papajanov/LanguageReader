namespace LanguageReader.Infrastructure.Features.ReadingItems.Parsing.Models;

public sealed record ParsedReadingAsset(
    string Id,
    string ContentType,
    string Base64Content);
