using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Users.Services;

namespace LanguageReader.Api.Features.Users;

internal sealed class CreateSessionHandler(
    UserAccountService userAccounts,
    PasswordHashService passwordHashService)
{
    public async Task<SessionDto> HandleAsync(LoginRequest request, CancellationToken ct)
    {
        var username = UsernameHelper.Require(request.Username);
        var account = await userAccounts.FindByUsernameAsync(username, ct);

        if (account is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Password)
                || !passwordHashService.Verify(request.Password, account.PasswordHash))
            {
                throw new ValidationException("Invalid username or password.");
            }

            return username.ToSessionDto();
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException("Invalid username or password.");
        }

        return username.ToSessionDto();
    }
}
