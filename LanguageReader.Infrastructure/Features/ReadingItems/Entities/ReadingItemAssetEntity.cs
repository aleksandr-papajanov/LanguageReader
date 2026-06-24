namespace LanguageReader.Infrastructure.Features.ReadingItems.Entities;

public sealed class ReadingItemAssetEntity
{
    public Guid ReadingItemId { get; set; }

    public string AssetId { get; set; } = string.Empty;

    public string Kind { get; set; } = "Image";

    public string ContentType { get; set; } = string.Empty;

    public string StoragePath { get; set; } = string.Empty;

    public string? AltText { get; set; }

    public int? Width { get; set; }

    public int? Height { get; set; }

    public bool IsCover { get; set; }

    public ReadingItemEntity? ReadingItem { get; set; }
}
