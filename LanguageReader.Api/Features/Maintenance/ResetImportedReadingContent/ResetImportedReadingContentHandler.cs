using System.ComponentModel.DataAnnotations;
using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace LanguageReader.Api.Features.Maintenance;

internal sealed class ResetImportedReadingContentHandler(
    ApplicationDbContext dbContext,
    IFileStorage storage)
{
    public const string RequiredConfirmation = "reset-imported-content";

    public async Task<ResetImportedReadingContentResult> HandleAsync(
        ResetImportedReadingContentRequest request,
        CancellationToken ct)
    {
        if (!string.Equals(request.Confirmation, RequiredConfirmation, StringComparison.Ordinal))
        {
            throw new ValidationException($"Pass confirmation='{RequiredConfirmation}' to reset imported reading content.");
        }

        var readingItems = await dbContext.ReadingItems.ToListAsync(ct);
        var legacyBookCount = await CountLegacyBooksAsync(ct);
        var storagePaths = await LoadStoragePathsAsync(ct);
        var rssCandidates = await dbContext.RssArticleCandidates
            .Where(candidate => candidate.SavedReadingItemId.HasValue)
            .ToListAsync(ct);
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

        await dbContext.SaveChangesAsync(ct);
        await DeleteLegacyBooksAsync(ct);

        var deleteFailures = new List<string>();
        var deletedFiles = 0;

        foreach (var storagePath in storagePaths)
        {
            try
            {
                await storage.DeleteAsync(storagePath, ct);
                deletedFiles++;
            }
            catch (Exception exception)
            {
                deleteFailures.Add($"{storagePath}: {exception.GetBaseException().Message}");
            }
        }

        return new ResetImportedReadingContentResult(
            readingItems.Count,
            legacyBookCount,
            rssCandidates.Count,
            deletedFiles,
            deleteFailures);
    }

    private async Task<IReadOnlyList<string>> LoadStoragePathsAsync(CancellationToken ct)
    {
        var readingItemSourcePaths = await dbContext.ReadingItems
            .Where(item => item.StoragePath != string.Empty)
            .Select(item => item.StoragePath)
            .ToListAsync(ct);
        var legacyBookSourcePaths = await LoadLegacyBookSourcePathsAsync(ct);
        var assetPaths = await LoadReadingItemAssetPathsAsync(ct);

        return readingItemSourcePaths
            .Concat(legacyBookSourcePaths)
            .Concat(assetPaths)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private Task<bool> LegacyBooksTableExistsAsync(CancellationToken ct)
    {
        return TableExistsAsync("books", ct);
    }

    private Task<bool> ReadingItemAssetsTableExistsAsync(CancellationToken ct)
    {
        return TableExistsAsync("reading_item_assets", ct);
    }

    private async Task<bool> TableExistsAsync(string tableName, CancellationToken ct)
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
            .FirstAsync(ct);

        return exists == 1;
    }

    private async Task<int> CountLegacyBooksAsync(CancellationToken ct)
    {
        if (!await LegacyBooksTableExistsAsync(ct))
        {
            return 0;
        }

        return await dbContext.Database
            .SqlQueryRaw<int>("select count(*)::int as \"Value\" from books")
            .FirstAsync(ct);
    }

    private async Task<IReadOnlyList<string>> LoadLegacyBookSourcePathsAsync(CancellationToken ct)
    {
        if (!await LegacyBooksTableExistsAsync(ct))
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
            .ToListAsync(ct);
    }

    private async Task<IReadOnlyList<string>> LoadReadingItemAssetPathsAsync(CancellationToken ct)
    {
        if (!await ReadingItemAssetsTableExistsAsync(ct))
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
            .ToListAsync(ct);
    }

    private async Task DeleteLegacyBooksAsync(CancellationToken ct)
    {
        if (!await LegacyBooksTableExistsAsync(ct))
        {
            return;
        }

        await dbContext.Database.ExecuteSqlRawAsync("delete from books", ct);
    }
}
