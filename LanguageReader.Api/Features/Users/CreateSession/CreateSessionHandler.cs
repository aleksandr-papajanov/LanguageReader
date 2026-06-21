using LanguageReader.Shared.Features.Users;

namespace LanguageReader.Api.Features.Users;

internal sealed class CreateSessionHandler
{
    public SessionDto Handle(LoginRequest request)
    {
        var username = UsernameHelper.Require(request.Username);
        return username.ToSessionDto();
    }
}
