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

    /// <summary>
    /// S3-compatible endpoint URL for Supabase Storage.
    /// </summary>
    public string? Endpoint { get; init; }

    /// <summary>
    /// S3 region from the Supabase Storage S3 settings page.
    /// </summary>
    public string? Region { get; init; }

    /// <summary>
    /// Server-side Supabase Storage S3 access key id.
    /// </summary>
    public string? AccessKeyId { get; init; }

    /// <summary>
    /// Server-side Supabase Storage S3 secret access key.
    /// </summary>
    public string? SecretAccessKey { get; init; }

    /// <summary>
    /// Use path-style addressing for S3-compatible providers.
    /// </summary>
    public bool ForcePathStyle { get; init; } = true;
}

