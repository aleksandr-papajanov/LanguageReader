using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Features.Ai;
using LanguageReader.Infrastructure.Features.Vocabulary.Entities;
using LanguageReader.Infrastructure.Features.Vocabulary.Models.Enrichment;

namespace LanguageReader.Infrastructure.Features.Vocabulary.Services;

public sealed class VocabularyAutofillApplicator(ApplicationDbContext dbContext)
{
    public void Apply(
        VocabularyEntryEntity entry,
        VocabularyAutofillResult generated,
        CancellationToken cancellationToken)
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
        if (!string.IsNullOrWhiteSpace(generated.PartOfSpeech))
        {
            entry.WordDetails.PartOfSpeech = generated.PartOfSpeech;
        }
        entry.WordDetails.Notes = generated.Notes;

        cancellationToken.ThrowIfCancellationRequested();

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

    public async Task ApplyAndSaveAsync(
        VocabularyEntryEntity entry,
        VocabularyAutofillResult generated,
        CancellationToken cancellationToken)
    {
        Apply(entry, generated, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
