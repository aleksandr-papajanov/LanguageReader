namespace LanguageReader.Shared.Features.Users;

public sealed record RegisterUserRequest(
    string Username,
    string? Email,
    string Password,
    string ConfirmPassword);
