using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.Users;

internal sealed class RegisterUserHandler(
    ApplicationDbContext dbContext,
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

        var exists = await dbContext.UserAccounts
            .AnyAsync(account =>
                account.Username == username
                || (email != null && account.Email == email),
                ct);

        if (exists)
        {
            throw new ValidationException("A user with this username or email already exists.");
        }

        dbContext.UserAccounts.Add(new UserAccountEntity
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHashService.Hash(request.Password),
            CreatedAtUtc = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(ct);
        return username.ToSessionDto();
    }

    private static string? NormalizeEmail(string? email)
    {
        var normalized = email?.Trim().ToLowerInvariant();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
