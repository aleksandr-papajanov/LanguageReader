using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Features.Common.Language;
using LanguageReader.Infrastructure.Features.ReadingItems.Entities;
using LanguageReader.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Infrastructure.Features.ReadingItems.Services;

public sealed class ReadingItemContentService(
    ApplicationDbContext dbContext,
    IFileStorage fileStorage) : IReadingItemContentService
{
    public async Task<ReadingItemContentPageDto> LoadPageAsync(
        ReadingItemEntity item,
        GetReadingItemContentRequest request,
        CancellationToken cancellationToken = default)
    {
        var document = await dbContext.ReadingItemDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.ReadingItemId == item.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Reading item '{item.Id}' does not have a canonical document.");
        var page = await ResolvePageAsync(item.Id, request, cancellationToken);
        var blockEntities = await dbContext.ReadingItemBlocks
            .AsNoTracking()
            .Where(block =>
                block.ReadingItemId == item.Id
                && block.SequenceIndex >= page.StartSequenceIndex
                && block.SequenceIndex <= page.EndSequenceIndex)
            .OrderBy(block => block.SequenceIndex)
            .ToListAsync(cancellationToken);
        var blocks = blockEntities
            .Select(block => new ReadingContentBlockDto(
                block.Type,
                block.Text,
                block.ImageId,
                block.BlockIndex))
            .ToArray();
        var imageIds = blocks
            .Select(block => block.ImageId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var images = await LoadImagesAsync(item.Id, imageIds, cancellationToken);

        return new ReadingItemContentPageDto(
            item.Id,
            item.Title,
            item.Type,
            LanguageNameNormalizer.Normalize(item.OriginalLanguage),
            page.PageIndex,
            page.TotalPages,
            page.StartBlockIndex,
            page.EndBlockIndex,
            document.TotalBlocks,
            blocks,
            images);
    }

    private async Task<ResolvedReadingItemPage> ResolvePageAsync(
        Guid readingItemId,
        GetReadingItemContentRequest request,
        CancellationToken cancellationToken)
    {
        var blockPlan = await dbContext.ReadingItemBlocks
            .AsNoTracking()
            .Where(block => block.ReadingItemId == readingItemId)
            .OrderBy(block => block.SequenceIndex)
            .Select(block => new ReadingItemBlockPageSource(
                block.SequenceIndex,
                block.BlockIndex,
                block.Weight))
            .ToListAsync(cancellationToken);
        var pages = BuildPages(blockPlan);

        if (pages.Count == 0)
        {
            return new ResolvedReadingItemPage(0, 1, 0, 0, 0, 0);
        }

        if (request.BlockIndex.HasValue)
        {
            var blockIndex = Math.Max(0, request.BlockIndex.Value);
            var pageByBlock = pages
                .OrderBy(page => page.PageIndex)
                .FirstOrDefault(page =>
                    page.StartBlockIndex <= blockIndex
                    && page.EndBlockIndex >= blockIndex);

            if (pageByBlock is not null)
            {
                return pageByBlock;
            }
        }

        var requestedPageIndex = Math.Max(0, request.PageIndex ?? 0);
        return pages[Math.Clamp(requestedPageIndex, 0, pages.Count - 1)];
    }

    private static IReadOnlyList<ResolvedReadingItemPage> BuildPages(
        IReadOnlyList<ReadingItemBlockPageSource> blocks)
    {
        if (blocks.Count == 0)
        {
            return [];
        }

        var pages = new List<ResolvedReadingItemPage>();
        var pageStartSequenceIndex = blocks[0].SequenceIndex;
        var pageWeight = 0;

        for (var index = 0; index < blocks.Count; index++)
        {
            var block = blocks[index];
            var blockWeight = Math.Max(0, block.Weight);
            var wouldOverfill = pageWeight > 0
                && pageWeight + blockWeight > ReadingItemContentPager.DefaultTargetPageWeight;
            var isLargeBlock = blockWeight >= ReadingItemContentPager.DefaultTargetPageWeight;

            if (wouldOverfill)
            {
                AddPage(pages, blocks, pageStartSequenceIndex, blocks[index - 1].SequenceIndex);
                pageStartSequenceIndex = block.SequenceIndex;
                pageWeight = 0;
            }

            pageWeight += blockWeight;

            if (isLargeBlock)
            {
                AddPage(pages, blocks, pageStartSequenceIndex, block.SequenceIndex);
                if (index + 1 < blocks.Count)
                {
                    pageStartSequenceIndex = blocks[index + 1].SequenceIndex;
                }

                pageWeight = 0;
            }
        }

        if (pages.Count == 0 || pages[^1].EndSequenceIndex < blocks[^1].SequenceIndex)
        {
            AddPage(pages, blocks, pageStartSequenceIndex, blocks[^1].SequenceIndex);
        }

        return pages
            .Select(page => page with { TotalPages = pages.Count })
            .ToArray();
    }

    private static void AddPage(
        List<ResolvedReadingItemPage> pages,
        IReadOnlyList<ReadingItemBlockPageSource> blocks,
        int startSequenceIndex,
        int endSequenceIndex)
    {
        var pageBlocks = blocks
            .Where(block => block.SequenceIndex >= startSequenceIndex && block.SequenceIndex <= endSequenceIndex)
            .ToArray();
        var addressableBlockIndexes = pageBlocks
            .Select(block => block.BlockIndex)
            .Where(blockIndex => blockIndex.HasValue)
            .Select(blockIndex => blockIndex!.Value)
            .ToArray();
        var startBlockIndex = addressableBlockIndexes.Length == 0 ? 0 : addressableBlockIndexes.Min();
        var endBlockIndex = addressableBlockIndexes.Length == 0 ? startBlockIndex : addressableBlockIndexes.Max();

        pages.Add(new ResolvedReadingItemPage(
            pages.Count,
            0,
            startSequenceIndex,
            endSequenceIndex,
            startBlockIndex,
            endBlockIndex));
    }

    private async Task<IReadOnlyDictionary<string, ReadingImageDto>> LoadImagesAsync(
        Guid readingItemId,
        IReadOnlySet<string> imageIds,
        CancellationToken cancellationToken)
    {
        if (imageIds.Count == 0)
        {
            return new Dictionary<string, ReadingImageDto>();
        }

        var assets = await dbContext.ReadingItemAssets
            .AsNoTracking()
            .Where(asset => asset.ReadingItemId == readingItemId && imageIds.Contains(asset.AssetId))
            .ToListAsync(cancellationToken);
        var images = new Dictionary<string, ReadingImageDto>(StringComparer.OrdinalIgnoreCase);

        foreach (var asset in assets)
        {
            await using var stream = await fileStorage.OpenReadAsync(asset.StoragePath, cancellationToken);
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory, cancellationToken);

            images[asset.AssetId] = new ReadingImageDto(
                asset.AssetId,
                asset.ContentType,
                Convert.ToBase64String(memory.ToArray()));
        }

        return images;
    }

    private sealed record ReadingItemBlockPageSource(
        int SequenceIndex,
        int? BlockIndex,
        int Weight);

    private sealed record ResolvedReadingItemPage(
        int PageIndex,
        int TotalPages,
        int StartSequenceIndex,
        int EndSequenceIndex,
        int StartBlockIndex,
        int EndBlockIndex);
}
