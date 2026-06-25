using LanguageReader.Infrastructure.Features.Common.Language;

namespace LanguageReader.Infrastructure.Ai.Operations.Translation.Tools;

public sealed class TranslationContextTool
{
    public TranslationContext GetContext(TranslateRequest request)
    {
        var context = string.IsNullOrWhiteSpace(request.OriginalText)
            ? request.SourceText.Trim()
            : request.OriginalText.Trim();

        var contextKind = request.SelectionKind switch
        {
            SelectionKind.Sentence => "sentence",
            SelectionKind.Paragraph => "block",
            _ => "selection_context"
        };

        return new TranslationContext(contextKind, context);
    }
}

public sealed record TranslationContext(
    string Kind,
    string Text);
