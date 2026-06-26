namespace LanguageReader.Client.Features.Reading.Services;

public sealed class ReaderSelectionService
{
    public ReaderSelection? BuildFromOffset(
        ReaderBlock paragraph,
        int offset,
        SelectionKind mode)
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
            _ => null
        };
    }

    public ReaderSelection? BuildCustomSelection(ReaderBlock paragraph, ReaderCustomSelectionRange range)
    {
        var startOffset = Math.Clamp(range.StartOffset, 0, paragraph.Text.Length);
        var endOffset = Math.Clamp(range.EndOffset, startOffset, paragraph.Text.Length);
        (startOffset, endOffset) = TrimSelectionBoundaries(paragraph.Text, startOffset, endOffset);

        if (endOffset <= startOffset)
        {
            return null;
        }

        var selectionType = ResolveCustomSelectionKind(paragraph, startOffset, endOffset);

        return new ReaderSelection(
            selectionType,
            paragraph.Text[startOffset..endOffset],
            paragraph.Index,
            startOffset,
            endOffset,
            startOffset,
            selectionType,
            null);
    }

    public ReaderSelection? ReselectFromAnchor(
        SelectionKind mode,
        int sourceBlockIndex,
        int sourceOffset,
        IReadOnlyList<ReaderBlock> allParagraphs)
    {
        var paragraph = allParagraphs.FirstOrDefault(paragraph => paragraph.Index == sourceBlockIndex)
            ?? allParagraphs.FirstOrDefault();

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
        IReadOnlyList<ReaderBlock> paragraphs,
        int targetBlockIndex,
        int targetOffset)
    {
        var paragraph = paragraphs.FirstOrDefault(item => item.Index == targetBlockIndex);
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

    public string? GetContextWindow(
        IReadOnlyList<ReaderBlock> paragraphs,
        int targetBlockIndex,
        int targetOffset,
        int sentenceRadius = 2)
    {
        var paragraph = paragraphs.FirstOrDefault(item => item.Index == targetBlockIndex);
        if (paragraph is null)
        {
            return null;
        }

        if (paragraph.Sentences.Count == 0)
        {
            return paragraph.Text.Trim();
        }

        var sentenceIndex = paragraph.Sentences
            .Select((sentence, index) => new { sentence, index })
            .FirstOrDefault(item => item.sentence.Contains(targetOffset))
            ?.index ?? 0;

        var startIndex = Math.Max(0, sentenceIndex - sentenceRadius);
        var endIndex = Math.Min(paragraph.Sentences.Count - 1, sentenceIndex + sentenceRadius);
        var start = Math.Clamp(paragraph.Sentences[startIndex].StartOffset, 0, paragraph.Text.Length);
        var end = Math.Clamp(paragraph.Sentences[endIndex].EndOffset, start, paragraph.Text.Length);

        return paragraph.Text[start..end].Trim();
    }

    private static ReaderSelection? BuildWordSelection(
        ReaderBlock paragraph,
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
        ReaderBlock paragraph,
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

    private static SelectionKind ResolveCustomSelectionKind(ReaderBlock paragraph, int startOffset, int endOffset)
    {
        var paragraphBounds = TrimSelectionBoundaries(paragraph.Text, 0, paragraph.Text.Length);
        if (startOffset == paragraphBounds.StartOffset && endOffset == paragraphBounds.EndOffset)
        {
            return SelectionKind.Paragraph;
        }

        if (paragraph.Words.Any(word => word.StartOffset == startOffset && word.EndOffset == endOffset))
        {
            return SelectionKind.Word;
        }

        foreach (var sentence in paragraph.Sentences)
        {
            var sentenceBounds = TrimSelectionBoundaries(paragraph.Text, sentence.StartOffset, sentence.EndOffset);
            if (startOffset == sentenceBounds.StartOffset && endOffset == sentenceBounds.EndOffset)
            {
                return SelectionKind.Sentence;
            }
        }

        return SelectionKind.Unknown;
    }

    private static (int StartOffset, int EndOffset) TrimSelectionBoundaries(string text, int startOffset, int endOffset)
    {
        while (startOffset < endOffset && IsBoundaryCharacter(text[startOffset]))
        {
            startOffset++;
        }

        while (endOffset > startOffset && IsBoundaryCharacter(text[endOffset - 1]))
        {
            endOffset--;
        }

        return (startOffset, endOffset);
    }

    private static bool IsBoundaryCharacter(char value)
    {
        return char.IsWhiteSpace(value)
            || char.IsPunctuation(value)
            || char.IsSymbol(value);
    }
}
