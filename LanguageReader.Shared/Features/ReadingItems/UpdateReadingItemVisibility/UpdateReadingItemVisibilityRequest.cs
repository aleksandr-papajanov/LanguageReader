namespace LanguageReader.Shared.Features.ReadingItems;

public sealed record UpdateReadingItemVisibilityRequest(
    Guid ReadingItemId,
    string Username,
    bool IsPublic);

public sealed record UpdateReadingItemVisibilityRequestRoute(
    Guid ReadingItemId);

public sealed record UpdateReadingItemVisibilityRequestBody(
    string Username,
    bool IsPublic);
