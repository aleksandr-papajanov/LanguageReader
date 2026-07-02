using LanguageReader.Infrastructure.Features.Vocabulary.Services;

namespace LanguageReader.Api.Features.Vocabulary;

internal sealed class GetVocabularyEntryHandler(VocabularyEntryGraphService vocabularyEntries)
{
    public async Task<VocabularyEntryDto> HandleAsync(GetVocabularyEntryRequest request, CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);
        var entry = await vocabularyEntries.LoadOwnedReadOnlyAsync(request.VocabularyId, normalizedUsername, ct);

        return entry.ToVocabularyEntryDto();
    }
}
