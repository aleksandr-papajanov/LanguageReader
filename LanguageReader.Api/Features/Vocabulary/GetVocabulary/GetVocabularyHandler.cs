using LanguageReader.Infrastructure.Features.Vocabulary.Services;

namespace LanguageReader.Api.Features.Vocabulary;

internal sealed class GetVocabularyHandler(VocabularyEntryGraphService vocabularyEntries)
{
    public async Task<IReadOnlyList<VocabularyEntryDto>> HandleAsync(GetVocabularyRequest request, CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);
        var rows = await vocabularyEntries.LoadUserEntriesAsync(
            normalizedUsername,
            request.IncludeHidden == true,
            ct);

        return rows.Select(entry => entry.ToVocabularyEntryDto()).ToList();
    }
}
