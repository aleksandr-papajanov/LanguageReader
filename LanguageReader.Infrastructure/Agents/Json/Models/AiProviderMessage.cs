namespace LanguageReader.Infrastructure.Agents.Json.Models;

public sealed record AiProviderMessage(
    AiMessageRole Role,
    string Content);