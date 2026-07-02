using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Ai;
using LanguageReader.Infrastructure.Features.ReadingItemTranslations.Entities;
using LanguageReader.Shared.Features.ReadingItemTranslations;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Infrastructure.Features.ReadingItemTranslations.Services;

public sealed class ReadingItemTranslationService(ApplicationDbContext dbContext)
{
    public async Task<IReadOnlyList<TranslatedRangeEntity>> LoadForReadingItemAsync(
        Guid readingItemId,
        string normalizedUsername,
        CancellationToken cancellationToken)
    {
        return await dbContext.TranslatedRanges
            .AsNoTracking()
            .Where(range => range.Username == normalizedUsername && range.ReadingItemId == readingItemId)
            .OrderBy(range => range.BlockIndex)
            .ThenBy(range => range.StartOffset)
            .ToListAsync(cancellationToken);
    }

    public async Task<TranslatedRangeEntity> CreateAsync(
        CreateTranslatedRangeRequest request,
        string normalizedUsername,
        SavedTextKind kind,
        CancellationToken cancellationToken)
    {
        var range = new TranslatedRangeEntity
        {
            Id = Guid.NewGuid(),
            Username = normalizedUsername,
            ReadingItemId = request.ReadingItemId,
            BlockIndex = request.BlockIndex,
            StartOffset = request.StartOffset,
            EndOffset = request.EndOffset,
            OriginalText = request.OriginalText.Trim(),
            TranslatedText = request.TranslatedText.Trim(),
            ShowOriginal = false,
            Kind = kind,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        dbContext.TranslatedRanges.Add(range);
        if (request.Usage is not null)
        {
            dbContext.AiOperations.Add(AiOperationMapper.ToEntity(
                request.Usage,
                normalizedUsername,
                translatedRangeId: range.Id));
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return range;
    }

    public async Task<TranslatedRangeEntity> UpdateDisplayAsync(
        Guid readingItemId,
        Guid translationId,
        string normalizedUsername,
        bool showOriginal,
        CancellationToken cancellationToken)
    {
        var range = await LoadOwnedAsync(readingItemId, translationId, normalizedUsername, cancellationToken);

        range.ShowOriginal = showOriginal;
        await dbContext.SaveChangesAsync(cancellationToken);

        return range;
    }

    public async Task DeleteAsync(
        Guid readingItemId,
        Guid translationId,
        string normalizedUsername,
        CancellationToken cancellationToken)
    {
        var range = await LoadOwnedAsync(readingItemId, translationId, normalizedUsername, cancellationToken);

        await dbContext.AiOperations
            .Where(operation => operation.TranslatedRangeId == range.Id && operation.VocabularyEntryId == null)
            .ExecuteDeleteAsync(cancellationToken);

        dbContext.TranslatedRanges.Remove(range);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<TranslatedRangeEntity> LoadOwnedAsync(
        Guid readingItemId,
        Guid translationId,
        string normalizedUsername,
        CancellationToken cancellationToken)
    {
        var range = await dbContext.TranslatedRanges.FirstOrDefaultAsync(
            item =>
                item.Id == translationId
                && item.ReadingItemId == readingItemId
                && item.Username == normalizedUsername,
            cancellationToken);

        return range
            ?? throw new NotFoundException($"Translated range '{translationId}' was not found.");
    }
}
