using LanguageReader.Infrastructure.Features.Vocabulary.Services;

namespace LanguageReader.Api.Features.Vocabulary;

internal sealed class UpdateVocabularyVisibilityHandler(
    VocabularyEntryGraphService vocabularyEntries,
    VocabularyVisibilityService visibility)
{
    public async Task<VocabularyEntryDto> HandleAsync(UpdateVocabularyVisibilityRequest request, CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);
        var entry = await vocabularyEntries.LoadOwnedAsync(request.VocabularyId, normalizedUsername, ct);

        await visibility.UpdateAsync(entry, request.IsVisibleInVocabulary, ct);

        return entry.ToVocabularyEntryDto();
    }
}
