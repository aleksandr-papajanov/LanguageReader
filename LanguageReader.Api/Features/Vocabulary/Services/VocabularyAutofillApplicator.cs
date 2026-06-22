using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Features.Ai;
using LanguageReader.Infrastructure.Features.Vocabulary.Entities;
using LanguageReader.Infrastructure.Features.Vocabulary.Models.Enrichment;

namespace LanguageReader.Api.Features.Vocabulary.Services;

internal sealed class VocabularyAutofillApplicator(ApplicationDbContext dbContext)
{
    public void Apply(
        VocabularyEntryEntity entry,
        VocabularyAutofillResult generated,
        CancellationToken ct)
    {
        entry.WordDetails ??= new VocabularyWordDetailsEntity
        {
            VocabularyEntryId = entry.Id
        };

        if (string.IsNullOrWhiteSpace(entry.WordDetails.SeenForm))
        {
            entry.WordDetails.SeenForm = entry.Word;
        }

        entry.WordDetails.DictionaryForm ??= entry.Word;

        if (!string.IsNullOrWhiteSpace(generated.PrimaryTranslation))
        {
            entry.Translation = generated.PrimaryTranslation.Trim();
        }

        entry.WordDetails.Description = generated.Description;
        entry.WordDetails.FrequencyScore = generated.FrequencyScore;
        entry.WordDetails.PartOfSpeech = generated.PartOfSpeech;
        entry.WordDetails.Notes = generated.Notes;

        ct.ThrowIfCancellationRequested();

        dbContext.RelatedWords.RemoveRange(entry.RelatedWords);
        entry.RelatedWords.Clear();

        var seeds = generated.AlternativeTranslations
            .Select(item => new VocabularyRelatedWordSeed(item, RelatedWordType.AlternativeTranslation))
            .Concat(generated.RelatedWords)
            .Where(item => !string.IsNullOrWhiteSpace(item.Word))
            .DistinctBy(item => (item.Word.Trim().ToLowerInvariant(), item.Type))
            .ToList();

        for (var index = 0; index < seeds.Count; index++)
        {
            var relatedWord = seeds[index];

            entry.RelatedWords.Add(new RelatedWordEntity
            {
                Id = Guid.NewGuid(),
                VocabularyEntryId = entry.Id,
                Word = relatedWord.Word.Trim(),
                Type = relatedWord.Type,
                SortOrder = index,
                CreatedAtUtc = DateTimeOffset.UtcNow
            });
        }

        dbContext.AiOperations.Add(AiOperationMapper.ToEntity(generated.Usage, entry.Username, vocabularyEntryId: entry.Id));
    }
}
