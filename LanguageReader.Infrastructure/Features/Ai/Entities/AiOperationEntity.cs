using LanguageReader.Infrastructure.Features.BookTranslations.Entities;
using LanguageReader.Infrastructure.Features.Vocabulary.Entities;

namespace LanguageReader.Infrastructure.Features.Ai.Entities;

/// <summary>
/// Persisted AI operation metadata for translations and vocabulary enrichment.
/// </summary>
public sealed class AiOperationEntity
{
    public Guid Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public AiOperationKind Kind { get; set; }

    public string Provider { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int InputTokens { get; set; }

    public int OutputTokens { get; set; }

    public int TotalTokens { get; set; }

    public decimal InputCostUsd { get; set; }

    public decimal OutputCostUsd { get; set; }

    public decimal TotalCostUsd { get; set; }

    public Guid? TranslatedRangeId { get; set; }

    public Guid? VocabularyEntryId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public TranslatedRangeEntity? TranslatedRange { get; set; }

    public VocabularyEntryEntity? VocabularyEntry { get; set; }
}
