namespace LanguageReader.Api.Features.Books;

internal sealed record CreateBookRequest(
    string Username,
    string Title,
    string OriginalLanguage,
    IFormFile? File);
