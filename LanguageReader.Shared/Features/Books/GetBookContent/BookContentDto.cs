namespace LanguageReader.Shared.Features.Books;

public sealed record BookContentDto(
    Guid Id,
    string Title,
    string Author,
    string OriginalLanguage,
    IReadOnlyList<BookContentBlockDto> Blocks,
    IReadOnlyDictionary<string, BookImageDto> Images);

public sealed record BookContentBlockDto(
    BookBlockType Type,
    string? Text,
    string? ImageId);

public sealed record BookImageDto(
    string Id,
    string ContentType,
    string Base64Content);

public enum BookBlockType
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