using LanguageReader.Infrastructure.Data;
using LanguageReader.Api.Features.Common.Services;
using LanguageReader.Api.Features.Settings.Services;

namespace LanguageReader.Api.Features.Settings;

internal sealed class UpdateUserSettingsHandler(
    ApplicationDbContext dbContext,
    UserSettingsAccessor userSettingsAccessor)
{
    public async Task<UserSettingsDto> HandleAsync(UpdateUserSettingsRequest request, CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);
        var settings = await userSettingsAccessor.GetOrCreateAsync(normalizedUsername, ct);
        settings.NativeLanguage = string.IsNullOrWhiteSpace(request.NativeLanguage) ? null : request.NativeLanguage.Trim();
        settings.AiServiceMode = request.AiServiceMode;

        await dbContext.SaveChangesAsync(ct);

        return settings.ToUserSettingsDto();
    }
}
