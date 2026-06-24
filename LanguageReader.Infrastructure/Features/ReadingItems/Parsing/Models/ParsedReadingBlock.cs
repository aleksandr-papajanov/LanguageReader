namespace LanguageReader.Infrastructure.Features.ReadingItems.Parsing.Models;

public sealed record ParsedReadingBlock(
    ReadingContentBlockType Type,
    string? Text,
    string? ImageId = null);
