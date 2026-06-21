using LanguageReader.Shared.Features.Users;

namespace LanguageReader.Client.Features.Users.Services;

public sealed class SessionApiClient(ApiClient api)
{
    public Task<SessionDto> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        return api.PostAsync<SessionDto>(
            "/api/session",
            body: request,
            ct: cancellationToken);
    }
}
