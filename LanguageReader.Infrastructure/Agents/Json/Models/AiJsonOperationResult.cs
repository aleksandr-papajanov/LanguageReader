using LanguageReader.Infrastructure.Agents.Providers.Models;

namespace LanguageReader.Infrastructure.Agents.Json.Models;

/// <summary>
/// Parsed direct-JSON AI result with provider diagnostics.
/// </summary>
public sealed record AiJsonOperationResult<TPayload>(
    TPayload Payload,
    string RawJson,
    string Model,
    string? ResponseId,
    AiProviderUsage? Usage,
    string? IncompleteReason);
