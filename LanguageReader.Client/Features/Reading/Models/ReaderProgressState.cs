namespace LanguageReader.Client.Features.Reading.Models;

public readonly record struct ReaderProgressState(
    int ParagraphsPerPage,
    int ParagraphCount,
    int PageCount,
    int PageIndex,
    int ParagraphIndex,
    int CharacterOffset)
{
    public int PageStartParagraphIndex => PageIndex * ParagraphsPerPage;

    public bool IsFirstPage => PageIndex <= 0;

    public bool IsLastPage => PageCount == 0 || PageIndex >= PageCount - 1;

    public double PagePercent => PageCount == 0
        ? 0
        : Math.Clamp(((double)(PageIndex + 1) / PageCount) * 100, 0, 100);

    public double ReadingPercent => ParagraphCount == 0
        ? 0
        : Math.Clamp(((double)Math.Min(ParagraphIndex + 1, ParagraphCount) / ParagraphCount) * 100, 0, 100);

    public static ReaderProgressState Empty(int paragraphsPerPage)
    {
        return new ReaderProgressState(paragraphsPerPage, 0, 0, 0, 0, 0);
    }

    public static ReaderProgressState Create(int paragraphsPerPage, int paragraphCount, int pageCount)
    {
        return Empty(paragraphsPerPage).WithDocument(paragraphCount, pageCount);
    }

    public int SafePageIndex(int pageCount)
    {
        return pageCount == 0 ? 0 : Math.Clamp(PageIndex, 0, pageCount - 1);
    }

    public ReaderProgressState WithDocument(int paragraphCount, int pageCount)
    {
        return this with
        {
            ParagraphCount = Math.Max(0, paragraphCount),
            PageCount = Math.Max(0, pageCount),
            PageIndex = pageCount == 0 ? 0 : Math.Clamp(PageIndex, 0, pageCount - 1),
            ParagraphIndex = paragraphCount == 0 ? 0 : Math.Clamp(ParagraphIndex, 0, paragraphCount - 1),
            CharacterOffset = Math.Max(0, CharacterOffset)
        };
    }

    public ReaderProgressState MoveTo(int paragraphIndex, int characterOffset, int paragraphCount, int pageCount)
    {
        if (paragraphCount <= 0 || pageCount <= 0)
        {
            return Empty(ParagraphsPerPage);
        }

        var safeParagraphIndex = Math.Clamp(paragraphIndex, 0, paragraphCount - 1);
        var safePageIndex = Math.Clamp(safeParagraphIndex / ParagraphsPerPage, 0, pageCount - 1);

        return this with
        {
            ParagraphCount = paragraphCount,
            PageCount = pageCount,
            PageIndex = safePageIndex,
            ParagraphIndex = safeParagraphIndex,
            CharacterOffset = Math.Max(0, characterOffset)
        };
    }

    public ReaderProgressState MoveToParagraph(int paragraphIndex, int paragraphCount, int pageCount)
    {
        return MoveTo(paragraphIndex, 0, paragraphCount, pageCount);
    }

    public ReaderProgressState MoveToPage(int pageIndex, int paragraphCount, int pageCount)
    {
        if (paragraphCount <= 0 || pageCount <= 0)
        {
            return Empty(ParagraphsPerPage);
        }

        var safePageIndex = Math.Clamp(pageIndex, 0, pageCount - 1);
        var paragraphIndex = Math.Clamp(safePageIndex * ParagraphsPerPage, 0, paragraphCount - 1);

        return this with
        {
            ParagraphCount = paragraphCount,
            PageCount = pageCount,
            PageIndex = safePageIndex,
            ParagraphIndex = paragraphIndex,
            CharacterOffset = 0
        };
    }
}
