using LanguageReader.Shared.Features.Books;

namespace LanguageReader.Shared.Features.ReadingItems;


public sealed record ReadingItemContentDto(
    Guid Id,
    string Title,
    ReadingItemType Type,
    string OriginalLanguage,
    IReadOnlyList<BookContentBlockDto> Blocks,
    IReadOnlyDictionary<string, BookImageDto> Images);
