using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using LanguageReader.Shared.Configuration;
using Microsoft.Extensions.Options;

namespace LanguageReader.Infrastructure.Storage;

/// <summary>
/// Supabase Storage implementation backed by the S3-compatible API.
/// </summary>
public sealed class SupabaseS3FileStorage : IFileStorage, IDisposable
{
    private readonly StorageOptions options;
    private readonly AmazonS3Client client;

    public SupabaseS3FileStorage(IOptions<StorageOptions> options)
    {
        this.options = options.Value;

        var endpoint = RequireOption(this.options.Endpoint, "Storage:Endpoint");
        var region = RequireOption(this.options.Region, "Storage:Region");
        var accessKeyId = RequireOption(this.options.AccessKeyId, "Storage:AccessKeyId");
        var secretAccessKey = RequireOption(this.options.SecretAccessKey, "Storage:SecretAccessKey");

        var config = new AmazonS3Config
        {
            ServiceURL = endpoint,
            AuthenticationRegion = region,
            ForcePathStyle = this.options.ForcePathStyle
        };

        client = new AmazonS3Client(
            new BasicAWSCredentials(accessKeyId, secretAccessKey),
            config);
    }

    /// <inheritdoc />
    public async Task<string> SaveAsync(string relativePath, Stream content, CancellationToken cancellationToken = default)
    {
        var key = NormalizeKey(relativePath);

        await client.PutObjectAsync(
            new PutObjectRequest
            {
                BucketName = RequireBucketName(),
                Key = key,
                InputStream = content,
                AutoCloseStream = false
            },
            cancellationToken);

        return key;
    }

    /// <inheritdoc />
    public async Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var response = await client.GetObjectAsync(
            RequireBucketName(),
            NormalizeKey(relativePath),
            cancellationToken);

        return new S3ObjectStream(response);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        await client.DeleteObjectAsync(
            RequireBucketName(),
            NormalizeKey(relativePath),
            cancellationToken);
    }

    public void Dispose()
    {
        client.Dispose();
    }

    private string RequireBucketName()
    {
        return RequireOption(options.BucketName, "Storage:BucketName");
    }

    private static string RequireOption(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{name} is required when Storage:Provider is Supabase.");
        }

        return value.Trim();
    }

    private static string NormalizeKey(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new InvalidOperationException("Storage paths are required.");
        }

        if (Path.IsPathRooted(relativePath))
        {
            throw new InvalidOperationException("Storage paths must be relative.");
        }

        var normalized = relativePath.Replace('\\', '/');
        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length == 0 || segments.Any(segment => segment is "." or ".."))
        {
            throw new InvalidOperationException("Storage paths must not contain relative directory segments.");
        }

        return string.Join('/', segments);
    }

    private sealed class S3ObjectStream(GetObjectResponse response) : Stream
    {
        private readonly Stream inner = response.ResponseStream;

        public override bool CanRead => inner.CanRead;

        public override bool CanSeek => inner.CanSeek;

        public override bool CanWrite => inner.CanWrite;

        public override long Length => inner.Length;

        public override long Position
        {
            get => inner.Position;
            set => inner.Position = value;
        }

        public override void Flush()
        {
            inner.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return inner.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return inner.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            inner.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            inner.Write(buffer, offset, count);
        }

        public override async ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            return await inner.ReadAsync(buffer, cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                inner.Dispose();
                response.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
