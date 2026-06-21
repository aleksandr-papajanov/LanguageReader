namespace LanguageReader.Shared.Configuration;

/// <summary>
/// Authentication configuration bound from the Authentication configuration section.
/// </summary>
public sealed class AuthenticationOptions
{
    /// <summary>
    /// Configuration section name for authentication settings.
    /// </summary>
    public const string SectionName = "Authentication";

    /// <summary>
    /// JWT authority for token validation.
    /// </summary>
    public string? Authority { get; init; }

    /// <summary>
    /// Expected JWT issuer.
    /// </summary>
    public string? Issuer { get; init; }

    /// <summary>
    /// Expected JWT audience.
    /// </summary>
    public string? Audience { get; init; }

    /// <summary>
    /// Indicates whether JWT metadata retrieval requires HTTPS.
    /// </summary>
    public bool RequireHttpsMetadata { get; init; } = true;
}

