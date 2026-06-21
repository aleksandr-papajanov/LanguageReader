namespace LanguageReader.Shared.Features.Books;

public sealed record BookDetailsDto(
    Guid Id,
    string Title,
    string Author,
    string CoverUrl,
    string OriginalLanguage,
    bool IsPublic,
    DateTimeOffset UploadedAtUtc);

