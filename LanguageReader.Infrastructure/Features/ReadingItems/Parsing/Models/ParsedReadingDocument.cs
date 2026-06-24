namespace LanguageReader.Infrastructure.Features.ReadingItems.Parsing.Models;

public sealed record ParsedReadingDocument(
    string? Title,
    IReadOnlyList<ParsedReadingBlock> Blocks,
    IReadOnlyDictionary<string, ParsedReadingAsset> Assets,
    string? CoverAssetId = null);
