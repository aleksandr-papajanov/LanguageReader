namespace LanguageReader.Client.Features.Reading.Services;

public sealed class ReaderSelectionService
{
    public ReaderSelection? BuildFromOffset(
        ReaderParagraph paragraph,
        int offset,
        SelectionKind mode,
        IReadOnlyList<ReaderParagraph> currentPageParagraphs,
        int pageStartParagraphIndex)
    {
        offset = Math.Clamp(offset, 0, paragraph.Text.Length);

        return mode switch
        {
            SelectionKind.Word => BuildWordSelection(paragraph, offset, mode),
            SelectionKind.Sentence => BuildSentenceSelection(paragraph, offset, mode),
            SelectionKind.Paragraph => new ReaderSelection(
                mode,
                paragraph.Text,
                paragraph.Index,
                0,
                paragraph.Text.Length,
                offset,
                SelectionKind.Paragraph,
                null),
            SelectionKind.Page => BuildPageSelection(currentPageParagraphs, pageStartParagraphIndex),
            _ => null
        };
    }

    public ReaderSelection? BuildCustomSelection(ReaderParagraph paragraph, ReaderCustomSelectionRange range)
    {
        var startOffset = Math.Clamp(range.StartOffset, 0, paragraph.Text.Length);
        var endOffset = Math.Clamp(range.EndOffset, startOffset, paragraph.Text.Length);

        if (endOffset <= startOffset)
        {
            return null;
        }

        return new ReaderSelection(
            SelectionKind.Phrase,
            paragraph.Text[startOffset..endOffset],
            paragraph.Index,
            startOffset,
            endOffset,
            startOffset,
            SelectionKind.Phrase,
            null);
    }

    public ReaderSelection BuildPageSelection(
        IReadOnlyList<ReaderParagraph> currentPageParagraphs,
        int pageStartParagraphIndex)
    {
        var text = string.Join(Environment.NewLine, currentPageParagraphs.Select(paragraph => paragraph.Text));
        var endParagraphIndex = (currentPageParagraphs.LastOrDefault()?.Index ?? pageStartParagraphIndex) + 1;

        return new ReaderSelection(
            SelectionKind.Page,
            text,
            pageStartParagraphIndex,
            0,
            endParagraphIndex,
            0,
            SelectionKind.Page,
            null);
    }

    public ReaderSelection? ReselectFromAnchor(
        SelectionKind mode,
        int sourceParagraphIndex,
        int sourceOffset,
        IReadOnlyList<ReaderParagraph> allParagraphs,
        IReadOnlyList<ReaderParagraph> currentPageParagraphs,
        int pageStartParagraphIndex)
    {
        if (mode == SelectionKind.Page)
        {
            return BuildPageSelection(currentPageParagraphs, pageStartParagraphIndex);
        }

        var paragraph = allParagraphs.FirstOrDefault(paragraph => paragraph.Index == sourceParagraphIndex)
            ?? currentPageParagraphs.FirstOrDefault();

        if (paragraph is null)
        {
            return null;
        }

        var word = paragraph.Words.FirstOrDefault(word => word.Contains(sourceOffset))
            ?? paragraph.Words.LastOrDefault(word => word.StartOffset <= sourceOffset)
            ?? paragraph.Words.FirstOrDefault();

        if (word is null)
        {
            return new ReaderSelection(
                mode,
                paragraph.Text,
                paragraph.Index,
                0,
                paragraph.Text.Length,
                sourceOffset,
                mode,
                null);
        }

        return mode switch
        {
            SelectionKind.Word => BuildWordSelection(paragraph, word.StartOffset, mode),
            SelectionKind.Sentence => BuildSentenceSelection(paragraph, word.StartOffset, mode),
            SelectionKind.Paragraph => new ReaderSelection(
                mode,
                paragraph.Text,
                paragraph.Index,
                0,
                paragraph.Text.Length,
                word.StartOffset,
                SelectionKind.Paragraph,
                null),
            _ => null
        };
    }

    public string? GetContextSentence(
        IReadOnlyList<ReaderParagraph> paragraphs,
        int targetParagraphIndex,
        int targetOffset)
    {
        var paragraph = paragraphs.FirstOrDefault(item => item.Index == targetParagraphIndex);
        if (paragraph is null)
        {
            return null;
        }

        var sentence = paragraph.Sentences.FirstOrDefault(item => item.Contains(targetOffset))
            ?? paragraph.Sentences.FirstOrDefault(item => item.StartOffset <= targetOffset && item.EndOffset >= targetOffset)
            ?? paragraph.Sentences.FirstOrDefault();

        if (sentence is null)
        {
            return null;
        }

        var start = Math.Clamp(sentence.StartOffset, 0, paragraph.Text.Length);
        var end = Math.Clamp(sentence.EndOffset, start, paragraph.Text.Length);
        return paragraph.Text[start..end].Trim();
    }

    private static ReaderSelection? BuildWordSelection(
        ReaderParagraph paragraph,
        int offset,
        SelectionKind activeMode)
    {
        var word = paragraph.Words.FirstOrDefault(word => word.Contains(offset))
            ?? paragraph.Words.LastOrDefault(word => word.StartOffset <= offset)
            ?? paragraph.Words.FirstOrDefault();

        return word is null
            ? null
            : new ReaderSelection(
                activeMode,
                paragraph.Text[word.StartOffset..word.EndOffset],
                paragraph.Index,
                word.StartOffset,
                word.EndOffset,
                word.StartOffset,
                SelectionKind.Word,
                null);
    }

    private static ReaderSelection BuildSentenceSelection(
        ReaderParagraph paragraph,
        int offset,
        SelectionKind activeMode)
    {
        var sentence = paragraph.Sentences.FirstOrDefault(sentence => sentence.Contains(offset));
        var startOffset = sentence?.StartOffset ?? 0;
        var endOffset = sentence?.EndOffset ?? paragraph.Text.Length;

        return new ReaderSelection(
            activeMode,
            paragraph.Text[startOffset..endOffset],
            paragraph.Index,
            startOffset,
            endOffset,
            offset,
            SelectionKind.Sentence,
            null);
    }
}
