namespace LanguageReader.Shared.Features.Books;

public sealed record GetBookContentRequest(
    Guid BookId,
    string? Username);

