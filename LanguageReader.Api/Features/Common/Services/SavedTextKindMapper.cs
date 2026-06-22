namespace LanguageReader.Api.Features.Common.Services;

internal static class SavedTextKindMapper
{
    public static SavedTextKind FromSelectionKind(SelectionKind selectionKind)
    {
        return selectionKind == SelectionKind.Word
            ? SavedTextKind.LexicalUnit
            : SavedTextKind.Text;
    }
}
