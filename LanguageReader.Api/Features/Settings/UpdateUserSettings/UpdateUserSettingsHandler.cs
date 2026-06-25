using LanguageReader.Infrastructure.Data;

namespace LanguageReader.Api.Features.Settings;

internal sealed class UpdateUserSettingsHandler(
    ApplicationDbContext dbContext,
    UserSettingsAccessor userSettingsAccessor)
{
    public async Task<UserSettingsDto> HandleAsync(UpdateUserSettingsRequest request, CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);
        var settings = await userSettingsAccessor.GetOrCreateAsync(normalizedUsername, ct);
        settings.NativeLanguage = SupportedLanguages.Normalize(request.NativeLanguage);

        await dbContext.SaveChangesAsync(ct);

        return settings.ToUserSettingsDto();
    }
}
