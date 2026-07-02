using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Vocabulary.Entities;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Infrastructure.Features.Vocabulary.Services;

public sealed class VocabularyEntryGraphService(ApplicationDbContext dbContext)
{
    public async Task<IReadOnlyList<VocabularyEntryEntity>> LoadUserEntriesAsync(
        string normalizedUsername,
        bool includeHidden,
        CancellationToken cancellationToken)
    {
        return await VocabularyEntryGraph(dbContext.VocabularyEntries.AsNoTracking())
            .Where(entry => entry.Username == normalizedUsername && (includeHidden || entry.IsVisibleInVocabulary))
            .OrderByDescending(entry => entry.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<VocabularyEntryEntity> LoadOwnedAsync(
        Guid id,
        string normalizedUsername,
        CancellationToken cancellationToken)
    {
        var entry = await LoadOwnedOrDefaultAsync(id, normalizedUsername, cancellationToken);

        return entry
            ?? throw new NotFoundException($"Vocabulary entry '{id}' was not found.");
    }

    public async Task<VocabularyEntryEntity?> LoadOwnedOrDefaultAsync(
        Guid id,
        string normalizedUsername,
        CancellationToken cancellationToken)
    {
        return await VocabularyEntryGraph(dbContext.VocabularyEntries)
            .FirstOrDefaultAsync(
                entry => entry.Id == id && entry.Username == normalizedUsername,
                cancellationToken);
    }

    public async Task<VocabularyEntryEntity> LoadOwnedReadOnlyAsync(
        Guid id,
        string normalizedUsername,
        CancellationToken cancellationToken)
    {
        var entry = await VocabularyEntryGraph(dbContext.VocabularyEntries.AsNoTracking())
            .FirstOrDefaultAsync(
                entry => entry.Id == id && entry.Username == normalizedUsername,
                cancellationToken);

        return entry
            ?? throw new NotFoundException($"Vocabulary entry '{id}' was not found.");
    }

    public async Task<VocabularyEntryEntity?> FindExistingAtPositionAsync(
        string normalizedUsername,
        SavedTextKind kind,
        string targetLanguage,
        Guid readingItemId,
        int blockIndex,
        int characterOffset,
        string canonicalText,
        CancellationToken cancellationToken)
    {
        var loweredCanonicalText = canonicalText.ToLowerInvariant();

        return await VocabularyEntryGraph(dbContext.VocabularyEntries)
            .FirstOrDefaultAsync(entry =>
                entry.Username == normalizedUsername
                && entry.ReadingItemId == readingItemId
                && entry.Word.ToLower() == loweredCanonicalText
                && entry.BlockIndex == blockIndex
                && entry.CharacterOffset == characterOffset
                && entry.Kind == kind
                && entry.TargetLanguage == targetLanguage,
                cancellationToken);
    }

    private static IQueryable<VocabularyEntryEntity> VocabularyEntryGraph(IQueryable<VocabularyEntryEntity> query)
    {
        return query
            .Include(entry => entry.ReadingItem)
            .Include(entry => entry.WordDetails)
            .Include(entry => entry.RelatedWords)
            .Include(entry => entry.AiOperations)
            .Include(entry => entry.Examples)
                .ThenInclude(example => example.ReadingItem);
    }
}
