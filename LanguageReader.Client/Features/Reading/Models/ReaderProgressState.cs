namespace LanguageReader.Client.Features.Reading.Models;

public readonly record struct ReaderProgressState(
    int TotalBlocks,
    int PageCount,
    int PageIndex,
    int ProgressBlockIndex,
    int BlockIndex,
    int CharacterOffset)
{
    public bool IsFirstPage => PageIndex <= 0;

    public bool IsLastPage => PageCount == 0 || PageIndex >= PageCount - 1;

    public double PagePercent => PageCount == 0
        ? 0
        : Math.Clamp(((double)(PageIndex + 1) / PageCount) * 100, 0, 100);

    public double ReadingPercent => TotalBlocks == 0
        ? 0
        : Math.Clamp(((double)Math.Min(ProgressBlockIndex + 1, TotalBlocks) / TotalBlocks) * 100, 0, 100);

    public static ReaderProgressState Empty()
    {
        return new ReaderProgressState(0, 0, 0, 0, 0, 0);
    }

    public static ReaderProgressState Create(
        int totalBlocks,
        int pageCount,
        int pageIndex,
        int blockIndex,
        int characterOffset = 0)
    {
        if (totalBlocks <= 0 || pageCount <= 0)
        {
            return Empty();
        }

        return new ReaderProgressState(
            Math.Max(0, totalBlocks),
            Math.Max(0, pageCount),
            Math.Clamp(pageIndex, 0, pageCount - 1),
            Math.Clamp(blockIndex, 0, totalBlocks - 1),
            Math.Clamp(blockIndex, 0, totalBlocks - 1),
            Math.Max(0, characterOffset));
    }

    public ReaderProgressState MoveTo(
        int blockIndex,
        int characterOffset,
        int totalBlocks,
        int pageCount,
        int pageIndex)
    {
        return Create(totalBlocks, pageCount, pageIndex, blockIndex, characterOffset);
    }

    public ReaderProgressState MoveToVisibleBlocks(int progressBlockIndex, int bookmarkBlockIndex)
    {
        if (TotalBlocks <= 0)
        {
            return Empty();
        }

        return this with
        {
            ProgressBlockIndex = Math.Clamp(progressBlockIndex, 0, TotalBlocks - 1),
            BlockIndex = Math.Clamp(bookmarkBlockIndex, 0, TotalBlocks - 1),
            CharacterOffset = 0
        };
    }

    public ReaderProgressState MoveToPage(
        int pageIndex,
        int startBlockIndex,
        int totalBlocks,
        int pageCount)
    {
        return Create(totalBlocks, pageCount, pageIndex, startBlockIndex);
    }
}
