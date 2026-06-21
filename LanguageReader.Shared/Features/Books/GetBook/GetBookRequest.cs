namespace LanguageReader.Shared.Features.Books;

public sealed record GetBookRequest(
    Guid BookId,
    string? Username);

