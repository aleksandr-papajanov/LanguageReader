using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Shared.Configuration;
using Microsoft.Extensions.Options;

namespace LanguageReader.Infrastructure.Agents.Json;

/// <summary>
/// Resolves the effective model for direct AI requests.
/// </summary>
public sealed class AiModelResolver(IOptions<OpenAiOptions> options) : IAiModelResolver
{
    private readonly OpenAiOptions openAiOptions = options.Value;

    public string Resolve(string? configuredModel)
    {
        var model = string.IsNullOrWhiteSpace(configuredModel)
            ? openAiOptions.DefaultModel
            : configuredModel;

        if (string.IsNullOrWhiteSpace(model))
        {
            throw new InfrastructureException("OpenAI model is not configured.");
        }

        return model.Trim();
    }
}
