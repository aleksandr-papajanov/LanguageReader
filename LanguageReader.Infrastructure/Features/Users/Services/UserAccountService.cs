using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Features.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Infrastructure.Features.Users.Services;

public sealed class UserAccountService(ApplicationDbContext dbContext)
{
    public async Task<UserAccountEntity?> FindByUsernameAsync(
        string username,
        CancellationToken cancellationToken)
    {
        return await dbContext.UserAccounts
            .FirstOrDefaultAsync(account => account.Username == username, cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        string username,
        string? email,
        CancellationToken cancellationToken)
    {
        return await dbContext.UserAccounts
            .AnyAsync(account =>
                account.Username == username
                || (email != null && account.Email == email),
                cancellationToken);
    }

    public async Task CreateAsync(
        string username,
        string? email,
        string passwordHash,
        CancellationToken cancellationToken)
    {
        dbContext.UserAccounts.Add(new UserAccountEntity
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            CreatedAtUtc = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
