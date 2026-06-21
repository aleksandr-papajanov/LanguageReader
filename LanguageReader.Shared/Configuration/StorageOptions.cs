namespace LanguageReader.Shared.Configuration;

/// <summary>
/// Storage configuration bound from the Storage configuration section.
/// </summary>
public sealed class StorageOptions
{
    /// <summary>
    /// Configuration section name for storage settings.
    /// </summary>
    public const string SectionName = "Storage";

    /// <summary>
    /// Storage provider identifier.
    /// </summary>
    public string Provider { get; init; } = "Local";

    /// <summary>
    /// Root path used by local file storage in development.
    /// </summary>
    public string LocalRootPath { get; init; } = "storage";

    /// <summary>
    /// Future bucket name for object storage providers.
    /// </summary>
    public string? BucketName { get; init; }
}

