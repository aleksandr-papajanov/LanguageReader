namespace LanguageReader.Client.Features.Reading.Models;

public sealed record ReaderSelection(
    SelectionKind Mode,
    string SelectedText,
    int BlockIndex,
    int StartOffset,
    int EndOffset,
    int AnchorOffset,
    SelectionKind SelectionType,
    string? TranslatedText);

public sealed record TapOption(
    string Label,
    ReaderSelection? Selection,
    TranslatedRangeDto? Translation);

public sealed record ReaderOffsetTap(ReaderBlock Block, int Offset);

public sealed record ReaderCustomSelectionRange(
    int BlockIndex,
    int StartOffset,
    int EndOffset,
    string SelectedText);

public sealed record ReaderRangeOverlay(
    string Id,
    string Kind,
    string Layer,
    int BlockIndex,
    int StartOffset,
    int EndOffset,
    string? DisplayText);

public sealed record ReaderRangeRect(
    string Id,
    string Kind,
    string Layer,
    int BlockIndex,
    int StartOffset,
    int EndOffset,
    string? DisplayText,
    double Left,
    double Top,
    double Width,
    double Height);

public sealed record ReaderDomTextHit(int BlockIndex, int Offset);

public sealed record ReaderVisibleBlocks(int ProgressBlockIndex, int BookmarkBlockIndex);

public sealed record ReaderViewportProgress(ReadingPositionDto Position, int ProgressBlockIndex);

public sealed record ReaderBlock(
    int Index,
    string Text,
    ReadingContentBlockType Type,
    string? ImageId,
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
