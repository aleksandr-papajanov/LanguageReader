namespace LanguageReader.Infrastructure.Features.ReadingItems.Entities;

public sealed class ReadingItemDocumentEntity
{
    public Guid ReadingItemId { get; set; }

    public int SchemaVersion { get; set; }

    public string ContentHash { get; set; } = string.Empty;

    public int TotalBlocks { get; set; }

    public int TotalPages { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public ReadingItemEntity? ReadingItem { get; set; }
}
