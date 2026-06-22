namespace LanguageReader.Shared.Features.Stats;

public sealed record GetAiOperationsStatsRequest(
    string Username,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc);
