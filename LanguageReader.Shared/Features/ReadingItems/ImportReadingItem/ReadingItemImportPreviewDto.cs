namespace LanguageReader.Shared.Features.ReadingItems;

public sealed record ReadingItemImportPreviewDto(
    string Title,
    ReadingItemType Type,
    string OriginalLanguage,
    string? SourceName,
    string? OriginalUrl,
    string? Author,
    DateTimeOffset? PublishedAtUtc,
    string? ImageUrl,
    string? Excerpt,
    int TextBlockCount,
    int ImageCount);
