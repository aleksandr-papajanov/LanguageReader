using LanguageReader.Shared.Features.ReadingItems;
using LanguageReader.Infrastructure.Features.BookTranslations.Entities;
using LanguageReader.Infrastructure.Features.Reading.Entities;
using LanguageReader.Infrastructure.Features.Vocabulary.Entities;

namespace LanguageReader.Infrastructure.Features.ReadingItems.Entities;

public sealed class ReadingItemEntity
{
    public Guid Id { get; set; }

    public string OwnerUsername { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string OriginalLanguage { get; set; } = "Unknown";

    public string StoragePath { get; set; } = string.Empty;

    public ReadingItemType Type { get; set; } = ReadingItemType.Book;

    public ReadingContentFormat ContentFormat { get; set; } = ReadingContentFormat.Fb2;

    public bool IsPublic { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public ArticleMetadataEntity? ArticleMetadata { get; set; }

    public ICollection<ReadingProgressEntity> ReadingProgresses { get; set; } = [];

    public ICollection<VocabularyEntryEntity> VocabularyEntries { get; set; } = [];

    public ICollection<TranslatedRangeEntity> TranslatedRanges { get; set; } = [];
}
