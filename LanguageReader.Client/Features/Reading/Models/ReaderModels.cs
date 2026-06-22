namespace LanguageReader.Client.Features.Reading.Models;

public sealed record ReaderSelection(
    SelectionKind Mode,
    string SelectedText,
    int ParagraphIndex,
    int StartOffset,
    int EndOffset,
    int AnchorOffset,
    SelectionKind SelectionType,
    string? TranslatedText);

public sealed record TapOption(
    string Label,
    ReaderSelection? Selection,
    TranslatedRangeDto? Translation);

public sealed record ReaderOffsetTap(ReaderParagraph Paragraph, int Offset);

public sealed record ReaderCustomSelectionRange(
    int ParagraphIndex,
    int StartOffset,
    int EndOffset,
    string SelectedText);

public sealed record ReaderRangeOverlay(
    string Id,
    string Kind,
    string Layer,
    int ParagraphIndex,
    int StartOffset,
    int EndOffset,
    string? DisplayText);

public sealed record ReaderRangeRect(
    string Id,
    string Kind,
    string Layer,
    int ParagraphIndex,
    int StartOffset,
    int EndOffset,
    string? DisplayText,
    double Left,
    double Top,
    double Width,
    double Height);

public sealed record ReaderDomTextHit(int ParagraphIndex, int Offset);

public sealed record ReaderParagraph(
    int Index,
    string Text,
    IReadOnlyList<InlineSegment> Segments,
    IReadOnlyList<WordToken> Words,
    IReadOnlyList<TextRange> Sentences);

public sealed record InlineSegment(
    string Text,
    int StartOffset,
    int EndOffset,
    WordToken? Word);

public sealed record WordToken(
    int Index,
    int StartOffset,
    int EndOffset,
    string Text)
{
    public bool Contains(int offset)
    {
        return offset >= StartOffset && offset < EndOffset;
    }
}

public sealed record TextRange(int StartOffset, int EndOffset)
{
    public bool Contains(int offset)
    {
        return offset >= StartOffset && offset < EndOffset;
    }
}
