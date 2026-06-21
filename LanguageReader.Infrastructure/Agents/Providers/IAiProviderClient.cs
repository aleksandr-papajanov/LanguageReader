using LanguageReader.Infrastructure.Agents.Providers.Models;

namespace LanguageReader.Infrastructure.Agents.Providers;

/// <summary>
/// Abstraction over a concrete AI provider HTTP client.
/// </summary>
public interface IAiProviderClient
{
    /// <summary>
    /// Sends a provider-neutral request to the configured AI provider.
    /// </summary>
    Task<AiProviderResponse> SendAsync(AiProviderRequest request, CancellationToken cancellationToken = default);
}

