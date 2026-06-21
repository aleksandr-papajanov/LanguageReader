using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.Users;

internal sealed class CreateSessionHandler(
    ApplicationDbContext dbContext,
    PasswordHashService passwordHashService)
{
    public async Task<SessionDto> HandleAsync(LoginRequest request, CancellationToken ct)
    {
        var username = UsernameHelper.Require(request.Username);
        var account = await dbContext.UserAccounts.FirstOrDefaultAsync(account => account.Username == username, ct);

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
