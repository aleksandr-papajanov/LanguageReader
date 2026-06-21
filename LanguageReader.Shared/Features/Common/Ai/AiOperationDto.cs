namespace LanguageReader.Shared.Features.Common;

public sealed record AiOperationDto(
    Guid Id,
    AiOperationKind Kind,
    string Provider,
    string Model,
    int InputTokens,
    int OutputTokens,
    int TotalTokens,
    decimal InputCostUsd,
    decimal OutputCostUsd,
    decimal TotalCostUsd,
    DateTimeOffset CreatedAtUtc);
