using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Features.Settings.Entities;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.Settings.Services;

internal sealed class UserSettingsAccessor(ApplicationDbContext dbContext)
{
    public async Task<UserSettingsEntity> GetOrCreateAsync(
        string username,
        CancellationToken cancellationToken)
    {
        var settings = await dbContext.UserSettings
            .FirstOrDefaultAsync(settings => settings.Username == username, cancellationToken);

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
}
