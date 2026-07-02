using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Users.Services;

namespace LanguageReader.Api.Features.Users;

internal sealed class RegisterUserHandler(
    UserAccountService userAccounts,
    PasswordHashService passwordHashService)
{
    public async Task<SessionDto> HandleAsync(RegisterUserRequest request, CancellationToken ct)
    {
        var username = UsernameHelper.Require(request.Username);
        var email = NormalizeEmail(request.Email);

        if (request.Password.Length < 6)
        {
            throw new ValidationException("Password must be at least 6 characters.");
        }

        if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
        {
            throw new ValidationException("Passwords do not match.");
        }

        var exists = await userAccounts.ExistsAsync(username, email, ct);

        if (exists)
        {
            throw new ValidationException("A user with this username or email already exists.");
        }

        await userAccounts.CreateAsync(username, email, passwordHashService.Hash(request.Password), ct);
        return username.ToSessionDto();
    }

    private static string? NormalizeEmail(string? email)
    {
        var normalized = email?.Trim().ToLowerInvariant();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
