using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Features.Reading.Entities;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Infrastructure.Features.Reading.Services;

public sealed class ReadingProgressService(ApplicationDbContext dbContext)
{
    public async Task<ReadingProgressEntity?> LoadReadOnlyAsync(
        string normalizedUsername,
        Guid readingItemId,
        CancellationToken cancellationToken)
    {
        return await dbContext.ReadingProgresses
            .AsNoTracking()
            .FirstOrDefaultAsync(progress =>
                progress.Username == normalizedUsername
                && progress.ReadingItemId == readingItemId,
                cancellationToken);
    }

    public async Task<Dictionary<Guid, ReadingProgressEntity>> LoadByItemIdAsync(
        IEnumerable<Guid> itemIds,
        string normalizedUsername,
        CancellationToken cancellationToken)
    {
        var ids = itemIds.Distinct().ToList();
        if (ids.Count == 0 || string.IsNullOrWhiteSpace(normalizedUsername))
        {
            return [];
        }

        return await dbContext.ReadingProgresses
            .AsNoTracking()
            .Where(progress => progress.Username == normalizedUsername && ids.Contains(progress.ReadingItemId))
            .ToDictionaryAsync(progress => progress.ReadingItemId, cancellationToken);
    }

    public async Task<ReadingProgressEntity> SaveAsync(
        string normalizedUsername,
        Guid readingItemId,
        double progressPercent,
        ReadingPositionDto position,
        CancellationToken cancellationToken)
    {
        var progress = await dbContext.ReadingProgresses
            .FirstOrDefaultAsync(item =>
                item.Username == normalizedUsername
                && item.ReadingItemId == readingItemId,
                cancellationToken);

        if (progress is null)
        {
            progress = new ReadingProgressEntity
            {
                Id = Guid.NewGuid(),
                Username = normalizedUsername,
                ReadingItemId = readingItemId
            };
            dbContext.ReadingProgresses.Add(progress);
        }

        progress.ProgressPercent = Math.Clamp(progressPercent, 0, 100);
        progress.BlockIndex = Math.Max(0, position.BlockIndex);
        progress.CharacterOffset = Math.Max(0, position.CharacterOffset);
        progress.LastOpenedAtUtc = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return progress;
    }
}
