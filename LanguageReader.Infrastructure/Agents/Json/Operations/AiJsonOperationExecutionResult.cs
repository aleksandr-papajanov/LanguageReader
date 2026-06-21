using LanguageReader.Infrastructure.Features.Ai.Models;

namespace LanguageReader.Infrastructure.Agents.Json.Operations;

public sealed record AiJsonOperationExecutionResult<TPayload>(
    TPayload Payload,
    string Model,
    string RawJson,
    AiOperationUsageDto Usage,
    string? ResponseId,
    string? IncompleteReason);
