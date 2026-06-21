namespace LanguageReader.Shared.Features.Common;

/// <summary>
/// Standard API error response.
/// </summary>
public sealed record ApiErrorResponse(
    string Type,
    string Message,
    int StatusCode,
    IReadOnlyDictionary<string, string[]>? Errors = null,
    string? TraceId = null,
    string? Detail = null);

