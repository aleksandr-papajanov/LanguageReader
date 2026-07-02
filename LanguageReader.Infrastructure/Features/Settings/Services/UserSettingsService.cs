using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Features.Settings.Entities;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Infrastructure.Features.Settings.Services;

public sealed class UserSettingsService(ApplicationDbContext dbContext)
{
    public async Task<UserSettingsEntity> GetOrCreateAsync(
        string username,
        CancellationToken cancellationToken)
    {
        var settings = await dbContext.UserSettings
            .FirstOrDefaultAsync(item => item.Username == username, cancellationToken);

        if (settings is not null)
        {
            return settings;
        }

        settings = new UserSettingsEntity
        {
            Username = username
        };

        dbContext.UserSettings.Add(settings);
        await dbContext.SaveChangesAsync(cancellationToken);

        return settings;
    }

    public async Task<UserSettingsEntity> UpdateNativeLanguageAsync(
        string username,
        string? nativeLanguage,
        CancellationToken cancellationToken)
    {
        var settings = await GetOrCreateAsync(username, cancellationToken);
        settings.NativeLanguage = SupportedLanguages.Normalize(nativeLanguage);

        await dbContext.SaveChangesAsync(cancellationToken);

        return settings;
    }
}
