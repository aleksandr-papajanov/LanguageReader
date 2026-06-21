namespace LanguageReader.Infrastructure.Features.Users.Entities;

/// <summary>
/// Persisted credentials for a registered application user.
/// </summary>
public sealed class UserAccountEntity
{
    public string Username { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }
}
