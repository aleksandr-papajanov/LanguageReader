namespace LanguageReader.Shared.Features.ReadingItems;


public sealed record ReadingItemContentPageDto(
    Guid Id,
    string Title,
    ReadingItemType Type,
    string OriginalLanguage,
    int PageIndex,
    int TotalPages,
    int StartBlockIndex,
    int EndBlockIndex,
    int TotalBlocks,
    IReadOnlyList<ReadingContentBlockDto> Blocks,
    IReadOnlyDictionary<string, ReadingImageDto> Images);

public sealed record ReadingContentBlockDto(
    ReadingContentBlockType Type,
    string? Text,
    string? ImageId,
    int? BlockIndex = null);

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
