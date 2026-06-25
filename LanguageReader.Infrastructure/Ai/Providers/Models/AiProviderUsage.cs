namespace LanguageReader.Infrastructure.Ai.Providers.Models;

/// <summary>
/// Real token usage returned by the upstream AI provider.
/// </summary>
public sealed record AiProviderUsage(
    int? InputTokens,
    int? OutputTokens,
    int? TotalTokens,
    int? ReasoningTokens = null,
    int? CachedInputTokens = null);
