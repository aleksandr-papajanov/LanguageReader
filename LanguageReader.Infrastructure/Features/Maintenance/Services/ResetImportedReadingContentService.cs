using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Features.Maintenance.Models;
using LanguageReader.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace LanguageReader.Infrastructure.Features.Maintenance.Services;

public sealed class ResetImportedReadingContentService(
    ApplicationDbContext dbContext,
    IFileStorage storage)
{
    public async Task<ResetImportedReadingContentSummary> ResetAsync(CancellationToken cancellationToken)
    {
        var readingItems = await dbContext.ReadingItems.ToListAsync(cancellationToken);
        var legacyBookCount = await CountLegacyBooksAsync(cancellationToken);
        var storagePaths = await LoadStoragePathsAsync(cancellationToken);
        var rssCandidates = await dbContext.RssArticleCandidates
            .Where(candidate => candidate.SavedReadingItemId.HasValue)
            .ToListAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;

        foreach (var candidate in rssCandidates)
        {
            candidate.SavedReadingItemId = null;
            candidate.Status = candidate.Status == NewsArticleStatus.Saved
                ? NewsArticleStatus.ExtractionSucceeded
                : candidate.Status;
            candidate.UpdatedAtUtc = now;
        }

        dbContext.ReadingItems.RemoveRange(readingItems);

        await dbContext.SaveChangesAsync(cancellationToken);
        await DeleteLegacyBooksAsync(cancellationToken);

        var deleteFailures = new List<string>();
        var deletedFiles = 0;

        foreach (var storagePath in storagePaths)
        {
            try
            {
                await storage.DeleteAsync(storagePath, cancellationToken);
                deletedFiles++;
            }
            catch (Exception exception)
            {
                deleteFailures.Add($"{storagePath}: {exception.GetBaseException().Message}");
            }
        }

        return new ResetImportedReadingContentSummary(
            readingItems.Count,
            legacyBookCount,
            rssCandidates.Count,
            deletedFiles,
            deleteFailures);
    }

    private async Task<IReadOnlyList<string>> LoadStoragePathsAsync(CancellationToken cancellationToken)
    {
        var readingItemSourcePaths = await dbContext.ReadingItems
            .Where(item => item.StoragePath != string.Empty)
            .Select(item => item.StoragePath)
            .ToListAsync(cancellationToken);
        var legacyBookSourcePaths = await LoadLegacyBookSourcePathsAsync(cancellationToken);
        var assetPaths = await LoadReadingItemAssetPathsAsync(cancellationToken);

        return readingItemSourcePaths
            .Concat(legacyBookSourcePaths)
            .Concat(assetPaths)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private Task<bool> LegacyBooksTableExistsAsync(CancellationToken cancellationToken)
    {
        return TableExistsAsync("books", cancellationToken);
    }

    private Task<bool> ReadingItemAssetsTableExistsAsync(CancellationToken cancellationToken)
    {
        return TableExistsAsync("reading_item_assets", cancellationToken);
    }

    private async Task<bool> TableExistsAsync(string tableName, CancellationToken cancellationToken)
    {
        var exists = await dbContext.Database
            .SqlQueryRaw<int>(
                """
                select case when exists (
                    select 1
                    from information_schema.tables
                    where table_schema = 'public' and table_name = @tableName
                ) then 1 else 0 end as "Value"
                """,
                new NpgsqlParameter("tableName", tableName))
            .FirstAsync(cancellationToken);

        return exists == 1;
    }

    private async Task<int> CountLegacyBooksAsync(CancellationToken cancellationToken)
    {
        if (!await LegacyBooksTableExistsAsync(cancellationToken))
        {
            return 0;
        }

        return await dbContext.Database
            .SqlQueryRaw<int>("select count(*)::int as \"Value\" from books")
            .FirstAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<string>> LoadLegacyBookSourcePathsAsync(CancellationToken cancellationToken)
    {
        if (!await LegacyBooksTableExistsAsync(cancellationToken))
        {
            return [];
        }

        return await dbContext.Database
            .SqlQueryRaw<string>(
                """
                select storage_path as "Value"
                from books
                where storage_path is not null and storage_path <> ''
                """)
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<string>> LoadReadingItemAssetPathsAsync(CancellationToken cancellationToken)
    {
        if (!await ReadingItemAssetsTableExistsAsync(cancellationToken))
        {
            return [];
        }

        return await dbContext.Database
            .SqlQueryRaw<string>(
                """
                select storage_path as "Value"
                from reading_item_assets
                where storage_path is not null and storage_path <> ''
                """)
            .ToListAsync(cancellationToken);
    }

    private async Task DeleteLegacyBooksAsync(CancellationToken cancellationToken)
    {
        if (!await LegacyBooksTableExistsAsync(cancellationToken))
        {
            return;
        }

        await dbContext.Database.ExecuteSqlRawAsync("delete from books", cancellationToken);
    }
}
