namespace LanguageReader.Shared.Features.Books;

public sealed record UpdateBookVisibilityRequest(
    Guid BookId,
    string Username,
    bool IsPublic);

public sealed record UpdateBookVisibilityRequestRoute(
    Guid BookId);

public sealed record UpdateBookVisibilityRequestBody(
    string Username,
    bool IsPublic);