namespace LanguageReader.Infrastructure.Ai.Providers.Models;

/// <summary>
/// Message sent to or received from an AI provider.
/// </summary>
public sealed record AiProviderChatMessage(
    AiProviderMessageRole Role,
    string Content);
