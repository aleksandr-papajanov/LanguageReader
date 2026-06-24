namespace LanguageReader.Shared.Features.ReadingItems;

public sealed record PreviewReadingItemUrlImportRequest(
    string Url,
    string OriginalLanguage);
