using LanguageReader.Shared.Features.Settings;

namespace LanguageReader.Infrastructure.Features.Ai.Settings;

/// <summary>
/// Resolves the effective AI mode for a user.
/// </summary>
public interface IUserAiServiceModeResolver
{
    Task<AiServiceMode> ResolveAsync(string username, CancellationToken cancellationToken = default);
}
