namespace LanguageReader.Shared.Configuration;

/// <summary>
/// Database configuration bound from the Database configuration section.
/// </summary>
public sealed class DatabaseOptions
{
    /// <summary>
    /// Configuration section name for database settings.
    /// </summary>
    public const string SectionName = "Database";

    /// <summary>
    /// PostgreSQL connection string.
    /// </summary>
    public string ConnectionString { get; init; } = string.Empty;

    /// <summary>
    /// Optional database command timeout in seconds.
    /// </summary>
    public int CommandTimeoutSeconds { get; init; } = 30;
}

