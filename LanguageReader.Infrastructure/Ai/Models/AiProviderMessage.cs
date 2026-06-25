namespace LanguageReader.Infrastructure.Ai.Models;

public sealed record AiProviderMessage(
    AiMessageRole Role,
    string Content);
