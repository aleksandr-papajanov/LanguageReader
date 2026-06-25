using LanguageReader.Infrastructure.Features.ReadingItemTranslations.Entities;
using LanguageReader.Infrastructure.Features.Vocabulary.Entities;

namespace LanguageReader.Infrastructure.Features.Ai.Entities;

/// <summary>
/// Persisted AI operation metadata for translations and vocabulary enrichment.
/// </summary>
public sealed class AiOperationEntity
{
    public Guid Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string OperationName { get; set; } = string.Empty;

    public string Provider { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string ExecutionMode { get; set; } = string.Empty;

    public int TurnCount { get; set; }

    public int ToolCallCount { get; set; }

    public string? ToolNames { get; set; }

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
