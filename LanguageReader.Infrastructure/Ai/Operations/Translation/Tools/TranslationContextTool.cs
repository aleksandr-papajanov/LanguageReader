using LanguageReader.Infrastructure.Features.Common.Language;

namespace LanguageReader.Infrastructure.Ai.Operations.Translation.Tools;

public sealed class TranslationContextTool
{
    public TranslationContext GetContext(TranslateRequest request)
    {
        var context = string.IsNullOrWhiteSpace(request.OriginalText)
            ? request.SourceText.Trim()
            : BuildExtendedContext(request.OriginalText, request.SourceText);

        var contextKind = request.SelectionKind switch
        {
            SelectionKind.Sentence => "sentence",
            SelectionKind.Paragraph => "block",
            _ => "extended_sentence_window"
        };

        return new TranslationContext(contextKind, context);
    }

    private static string BuildExtendedContext(string originalText, string selectedText)
    {
        var context = originalText.Trim();
        if (string.IsNullOrWhiteSpace(context) || string.IsNullOrWhiteSpace(selectedText))
        {
            return context;
        }

        var sentences = SplitSentences(context);
        if (sentences.Count <= 1)
        {
            return context;
        }

        var selectedIndex = sentences.FindIndex(sentence =>
            sentence.Contains(selectedText.Trim(), StringComparison.OrdinalIgnoreCase));

        if (selectedIndex < 0)
        {
            return context;
        }

        var start = Math.Max(0, selectedIndex - 2);
        var count = Math.Min(sentences.Count - start, 5);

        return string.Join(" ", sentences.Skip(start).Take(count));
    }

    private static List<string> SplitSentences(string text)
    {
        var sentences = new List<string>();
        var start = 0;

        for (var index = 0; index < text.Length; index++)
        {
            if (text[index] is not ('.' or '!' or '?' or '।' or '؟'))
            {
                continue;
            }

            var sentence = text[start..(index + 1)].Trim();
            if (!string.IsNullOrWhiteSpace(sentence))
            {
                sentences.Add(sentence);
            }

            start = index + 1;
        }

        var tail = text[start..].Trim();
        if (!string.IsNullOrWhiteSpace(tail))
        {
            sentences.Add(tail);
        }

        return sentences;
    }
}

public sealed record TranslationContext(
    string Kind,
    string Text);
