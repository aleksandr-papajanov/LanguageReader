namespace LanguageReader.Shared.Features.ReadingItems;


public sealed record ReadingItemContentDto(
    Guid Id,
    string Title,
    ReadingItemType Type,
    string OriginalLanguage,
    IReadOnlyList<ReadingContentBlockDto> Blocks,
    IReadOnlyDictionary<string, ReadingImageDto> Images);

public sealed record ReadingContentBlockDto(
    ReadingContentBlockType Type,
    string? Text,
    string? ImageId);

public sealed record ReadingImageDto(
    string Id,
    string ContentType,
    string Base64Content);

public enum ReadingContentBlockType
{
    Paragraph,
    Heading1,
    Heading2,
    Quote,
    Verse,
    Author,
    EmptyLine,
    Image
}
