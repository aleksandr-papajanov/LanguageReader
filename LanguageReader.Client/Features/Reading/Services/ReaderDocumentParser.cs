using System.Text.RegularExpressions;

namespace LanguageReader.Client.Features.Reading.Services;

public static class ReaderDocumentParser
{
    private static readonly Regex WordRegex = new(
        @"[\p{L}\p{N}]+(?:['\u2019-][\p{L}\p{N}]+)*",
        RegexOptions.Compiled);

    public static IReadOnlyList<ReaderParagraph> BuildParagraphs(
        IReadOnlyList<ReadingContentBlockDto> blocks)
    {
        return blocks
            .Select((block, index) => BuildParagraph(index, block))
            .ToList();
    }

    public static IReadOnlyList<IReadOnlyList<ReaderParagraph>> BuildPages(
        IReadOnlyList<ReaderParagraph> paragraphs,
        int paragraphsPerPage)
    {
        var pages = new List<IReadOnlyList<ReaderParagraph>>();

        for (var i = 0; i < paragraphs.Count; i += paragraphsPerPage)
        {
            pages.Add(paragraphs.Skip(i).Take(paragraphsPerPage).ToList());
        }

        return pages;
    }

    public static string? GetSingleWord(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var trimmed = text.Trim();
        var matches = WordRegex.Matches(trimmed);

        if (matches.Count != 1)
        {
            return null;
        }

        var match = matches[0];
        return match.Index == 0 && match.Length == trimmed.Length
            ? match.Value
            : null;
    }

    private static ReaderParagraph BuildParagraph(int index, ReadingContentBlockDto block)
    {
        var text = block.Text ?? string.Empty;

        if (!CanTokenize(block.Type))
        {
            return new ReaderParagraph(
                index,
                text,
                block.Type,
                block.ImageId,
                Segments: [],
                Words: [],
                Sentences: []);
        }

        var words = WordRegex.Matches(text)
            .Select((match, wordIndex) => new WordToken(
                wordIndex,
                match.Index,
                match.Index + match.Length,
                match.Value))
            .ToList();

        var sentences = BuildSentences(text);
        var segments = BuildSegments(text, words);

        return new ReaderParagraph(
            index,
            text,
            block.Type,
            block.ImageId,
            segments,
            words,
            sentences);
    }

    private static bool CanTokenize(ReadingContentBlockType type)
    {
        return type is
            ReadingContentBlockType.Paragraph or
            ReadingContentBlockType.Heading1 or
            ReadingContentBlockType.Heading2 or
            ReadingContentBlockType.Quote or
            ReadingContentBlockType.Verse or
            ReadingContentBlockType.Author;
    }

    private static IReadOnlyList<InlineSegment> BuildSegments(
        string text,
        IReadOnlyList<WordToken> words)
    {
        var segments = new List<InlineSegment>();
        var cursor = 0;

        foreach (var word in words)
        {
            if (word.StartOffset > cursor)
            {
                segments.Add(new InlineSegment(
                    text[cursor..word.StartOffset],
                    cursor,
                    word.StartOffset,
                    null));
            }

            segments.Add(new InlineSegment(
                text[word.StartOffset..word.EndOffset],
                word.StartOffset,
                word.EndOffset,
                word));

            cursor = word.EndOffset;
        }

        if (cursor < text.Length)
        {
            segments.Add(new InlineSegment(
                text[cursor..],
                cursor,
                text.Length,
                null));
        }

        return segments;
    }

    private static IReadOnlyList<TextRange> BuildSentences(string text)
    {
        var sentences = new List<TextRange>();
        var start = 0;

        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] is not ('.' or '!' or '?'))
            {
                continue;
            }

            var end = i + 1;

            while (end < text.Length && char.IsWhiteSpace(text[end]))
            {
                end++;
            }

            AddSentence(sentences, text, start, end);
            start = end;
        }

        AddSentence(sentences, text, start, text.Length);

        return sentences.Count == 0
            ? [new TextRange(0, text.Length)]
            : sentences;
    }

    private static void AddSentence(
        List<TextRange> sentences,
        string text,
        int start,
        int end)
    {
        while (start < end && char.IsWhiteSpace(text[start]))
        {
            start++;
        }

        while (end > start && char.IsWhiteSpace(text[end - 1]))
        {
            end--;
        }

        if (end > start)
        {
            sentences.Add(new TextRange(start, end));
        }
    }
}
