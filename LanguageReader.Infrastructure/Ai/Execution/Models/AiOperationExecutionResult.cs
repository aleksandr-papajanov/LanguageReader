namespace LanguageReader.Infrastructure.Ai.Execution;

public sealed record AiOperationExecutionResult<TPayload>(
    TPayload Payload,
    string Model,
    string RawJson,
    AiOperationUsageDto Usage,
    string? ResponseId,
    string? IncompleteReason);
