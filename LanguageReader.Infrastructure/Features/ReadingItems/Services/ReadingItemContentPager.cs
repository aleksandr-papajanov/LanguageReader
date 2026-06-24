namespace LanguageReader.Infrastructure.Features.ReadingItems.Services;

internal static class ReadingItemContentPager
{
    public const int DefaultTargetPageWeight = 6200;

    public static IReadOnlyList<ReadingItemContentPagePlan> BuildPagePlan(
        IReadOnlyList<ReadingContentBlockDto> blocks,
        int targetPageWeight = DefaultTargetPageWeight)
    {
        if (blocks.Count == 0)
        {
            return [new ReadingItemContentPagePlan(0, 0, 0, 0, 0, 0)];
        }

        var pages = BuildPages(blocks, Math.Max(1200, targetPageWeight));

        return pages
            .Select((page, pageIndex) =>
            {
                var pageBlocks = blocks
                    .Skip(page.StartBlockIndex)
                    .Take(page.EndBlockIndex - page.StartBlockIndex + 1)
                    .ToArray();
                var addressableBlockIndexes = pageBlocks
                    .Select(block => block.BlockIndex)
                    .Where(index => index.HasValue)
                    .Select(index => index!.Value)
                    .ToArray();

                return new ReadingItemContentPagePlan(
                    pageIndex,
                    page.StartBlockIndex,
                    page.EndBlockIndex,
                    addressableBlockIndexes.Length == 0 ? 0 : addressableBlockIndexes.Min(),
                    addressableBlockIndexes.Length == 0 ? 0 : addressableBlockIndexes.Max(),
                    pageBlocks.Sum(GetBlockWeight));
            })
            .ToArray();
    }

    public static int CalculateBlockWeight(ReadingContentBlockDto block)
    {
        return GetBlockWeight(block);
    }

    public static ReadingItemContentPage Slice(
        IReadOnlyList<ReadingContentBlockDto> blocks,
        int? requestedPageIndex,
        int? requestedBlockIndex,
        int? requestedTargetPageWeight)
    {
        if (blocks.Count == 0)
        {
            return new ReadingItemContentPage(0, 0, 0, 0, 0, []);
        }

        var targetPageWeight = Math.Max(1200, requestedTargetPageWeight ?? DefaultTargetPageWeight);
        var pages = BuildPages(blocks, targetPageWeight);
        var pageIndex = ResolvePageIndex(blocks, pages, requestedPageIndex, requestedBlockIndex);
        var page = pages[pageIndex];

        var pageBlocks = blocks
            .Skip(page.StartBlockIndex)
            .Take(page.EndBlockIndex - page.StartBlockIndex + 1)
            .ToArray();
        var addressableBlockIndexes = pageBlocks
            .Select(block => block.BlockIndex)
            .Where(index => index.HasValue)
            .Select(index => index!.Value)
            .ToArray();

        return new ReadingItemContentPage(
            pageIndex,
            pages.Count,
            addressableBlockIndexes.Length == 0 ? 0 : addressableBlockIndexes.Min(),
            addressableBlockIndexes.Length == 0 ? 0 : addressableBlockIndexes.Max(),
            blocks.Count(block => block.BlockIndex.HasValue),
            pageBlocks);
    }

    private static IReadOnlyList<ReadingItemContentPageBoundary> BuildPages(
        IReadOnlyList<ReadingContentBlockDto> blocks,
        int targetPageWeight)
    {
        var pages = new List<ReadingItemContentPageBoundary>();
        var pageStartIndex = 0;
        var pageWeight = 0;

        for (var index = 0; index < blocks.Count; index++)
        {
            var blockWeight = GetBlockWeight(blocks[index]);
            var wouldOverfill = pageWeight > 0 && pageWeight + blockWeight > targetPageWeight;
            var isLargeBlock = blockWeight >= targetPageWeight;

            if (wouldOverfill)
            {
                pages.Add(new ReadingItemContentPageBoundary(pageStartIndex, index - 1));
                pageStartIndex = index;
                pageWeight = 0;
            }

            pageWeight += blockWeight;

            if (isLargeBlock)
            {
                pages.Add(new ReadingItemContentPageBoundary(pageStartIndex, index));
                pageStartIndex = index + 1;
                pageWeight = 0;
            }
        }

        if (pageStartIndex < blocks.Count)
        {
            pages.Add(new ReadingItemContentPageBoundary(pageStartIndex, blocks.Count - 1));
        }

        return pages.Count == 0
            ? [new ReadingItemContentPageBoundary(0, blocks.Count - 1)]
            : pages;
    }

    private static int ResolvePageIndex(
        IReadOnlyList<ReadingContentBlockDto> blocks,
        IReadOnlyList<ReadingItemContentPageBoundary> pages,
        int? requestedPageIndex,
        int? requestedBlockIndex)
    {
        if (requestedBlockIndex.HasValue)
        {
            var blockIndex = Math.Max(0, requestedBlockIndex.Value);
            var blockPageIndex = pages
                .Select((page, index) => new { page, index })
                .FirstOrDefault(candidate =>
                    blocks
                        .Skip(candidate.page.StartBlockIndex)
                        .Take(candidate.page.EndBlockIndex - candidate.page.StartBlockIndex + 1)
                        .Any(block => block.BlockIndex == blockIndex))
                ?.index;

            if (blockPageIndex.HasValue)
            {
                return blockPageIndex.Value;
            }
        }

        return Math.Clamp(requestedPageIndex ?? 0, 0, pages.Count - 1);
    }

    private static int GetBlockWeight(ReadingContentBlockDto block)
    {
        return block.Type switch
        {
            ReadingContentBlockType.Heading1 => 900 + GetTextWeight(block.Text, 1.4),
            ReadingContentBlockType.Heading2 => 650 + GetTextWeight(block.Text, 1.25),
            ReadingContentBlockType.Quote => 250 + GetTextWeight(block.Text, 1.15),
            ReadingContentBlockType.Verse => 180 + GetTextWeight(block.Text, 1.1),
            ReadingContentBlockType.Author => 140 + GetTextWeight(block.Text, 1),
            ReadingContentBlockType.EmptyLine => 140,
            ReadingContentBlockType.Image => 1500,
            _ => 180 + GetTextWeight(block.Text, 1)
        };
    }

    private static int GetTextWeight(string? text, double multiplier)
    {
        return string.IsNullOrWhiteSpace(text)
            ? 0
            : (int)Math.Ceiling(text.Length * multiplier);
    }

    private sealed record ReadingItemContentPageBoundary(int StartBlockIndex, int EndBlockIndex);
}

internal sealed record ReadingItemContentPage(
    int PageIndex,
    int TotalPages,
    int StartBlockIndex,
    int EndBlockIndex,
    int TotalBlocks,
    IReadOnlyList<ReadingContentBlockDto> Blocks);

public sealed record ReadingItemContentPagePlan(
    int PageIndex,
    int StartSequenceIndex,
    int EndSequenceIndex,
    int StartBlockIndex,
    int EndBlockIndex,
    int Weight);
