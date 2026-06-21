namespace LanguageReader.Shared.Features.Reading;

public sealed record SaveReadingProgressRequest(
    Guid ReadingItemId,
    string Username,
    double ProgressPercent,
    ReadingPositionDto Position);

public sealed record SaveReadingProgressRequestRoute(Guid ReadingItemId);

public sealed record SaveReadingProgressRequestBody(
    string Username,
    double ProgressPercent,
    ReadingPositionDto Position);
