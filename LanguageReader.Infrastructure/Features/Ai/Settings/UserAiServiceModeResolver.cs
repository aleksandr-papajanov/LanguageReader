using LanguageReader.Infrastructure.Data;
using LanguageReader.Shared.Features.Settings;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Infrastructure.Features.Ai.Settings;

/// <summary>
/// Reads the persisted AI mode for the current user.
/// </summary>
public sealed class UserAiServiceModeResolver(ApplicationDbContext dbContext) : IUserAiServiceModeResolver
{
    public async Task<AiServiceMode> ResolveAsync(string username, CancellationToken cancellationToken = default)
    {
        var normalizedUsername = username?.Trim().ToLowerInvariant() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedUsername))
        {
            return AiServiceMode.Fake;
        }

        var mode = await dbContext.UserSettings
            .Where(item => item.Username == normalizedUsername)
            .Select(item => (AiServiceMode?)item.AiServiceMode)
            .FirstOrDefaultAsync(cancellationToken);

        return mode ?? AiServiceMode.Fake;
    }
}
