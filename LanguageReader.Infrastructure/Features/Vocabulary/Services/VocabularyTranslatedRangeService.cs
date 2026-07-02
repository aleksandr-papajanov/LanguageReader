using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Features.ReadingItemTranslations.Entities;
using LanguageReader.Infrastructure.Features.Vocabulary.Entities;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Infrastructure.Features.Vocabulary.Services;

public sealed class VocabularyTranslatedRangeService(ApplicationDbContext dbContext)
{
    public async Task<TranslatedRangeEntity?> FindMatchingAsync(
        Guid readingItemId,
        string normalizedUsername,
        SavedTextKind kind,
        int blockIndex,
        int characterOffset,
        string text,
        CancellationToken cancellationToken)
    {
        var normalizedText = text.Trim().ToLowerInvariant();

        var exactMatch = await dbContext.TranslatedRanges
            .OrderByDescending(range => range.CreatedAtUtc)
            .FirstOrDefaultAsync(range =>
                range.Username == normalizedUsername
                && range.ReadingItemId == readingItemId
                && range.BlockIndex == blockIndex
                && range.StartOffset == characterOffset
                && range.Kind == kind
                && range.OriginalText.ToLower() == normalizedText,
                cancellationToken);

        if (exactMatch is not null)
        {
            return exactMatch;
        }

        return await dbContext.TranslatedRanges
            .OrderByDescending(range => range.CreatedAtUtc)
            .FirstOrDefaultAsync(range =>
                range.Username == normalizedUsername
                && range.ReadingItemId == readingItemId
                && range.BlockIndex == blockIndex
                && range.StartOffset == characterOffset
                && range.OriginalText.ToLower() == normalizedText,
                cancellationToken);
    }

    public async Task LinkToEntryAsync(
        TranslatedRangeEntity? range,
        VocabularyEntryEntity entry,
        CancellationToken cancellationToken)
    {
        if (range is null)
        {
            return;
        }

        range.VocabularyEntryId = entry.Id;
        range.Kind = entry.Kind;

        await dbContext.Entry(range)
            .Collection(item => item.AiOperations)
            .LoadAsync(cancellationToken);

        foreach (var operation in range.AiOperations.Where(item => item.VocabularyEntryId != entry.Id))
        {
            operation.VocabularyEntryId = entry.Id;
        }
    }
}
