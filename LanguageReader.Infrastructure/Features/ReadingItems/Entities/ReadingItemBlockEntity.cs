namespace LanguageReader.Infrastructure.Features.ReadingItems.Entities;

public sealed class ReadingItemBlockEntity
{
    public Guid ReadingItemId { get; set; }

    public int SequenceIndex { get; set; }

    public int? BlockIndex { get; set; }

    public ReadingContentBlockType Type { get; set; }

    public string? Text { get; set; }

    public string? ImageId { get; set; }

    public int Weight { get; set; }

    public ReadingItemEntity? ReadingItem { get; set; }
}
