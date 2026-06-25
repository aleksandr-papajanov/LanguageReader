using LanguageReader.Shared.Configuration;
using Microsoft.Extensions.Options;

namespace LanguageReader.Infrastructure.Ai.Execution;

public sealed class AiModelResolver(
    IOptions<OpenAiOptions> options) : IAiModelResolver
{
    public string Resolve(string? requestedModel)
    {
        if (!string.IsNullOrWhiteSpace(requestedModel))
        {
            return requestedModel.Trim();
        }

        var configuredModel = options.Value.DefaultModel;
        if (!string.IsNullOrWhiteSpace(configuredModel))
        {
            return configuredModel.Trim();
        }

        return "gpt-5-mini";
    }
}
