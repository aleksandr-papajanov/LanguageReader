namespace LanguageReader.Shared.Features.Books;

public sealed record DeleteBookRequest(
    Guid BookId,
    string Username);

