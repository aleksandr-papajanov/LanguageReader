using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Ai;
using LanguageReader.Infrastructure.Features.Vocabulary.Entities;
using LanguageReader.Infrastructure.Features.Vocabulary.Models.Enrichment;
using LanguageReader.Infrastructure.Features.Vocabulary.Services.Enrichment;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.Vocabulary;

internal sealed class AutofillVocabularyEntryHandler(
    ApplicationDbContext dbContext,
    IVocabularyEnrichmentService enrichmentService)
{
    public async Task<VocabularyEntryDto> HandleAsync(AutofillVocabularyEntryRequest request, CancellationToken ct)
    {
        var entry = await LoadOwnedEntryAsync(request.VocabularyId, request.Username, ct);

        if (entry.SelectionKind != SelectionKind.Word)
        {
            throw new ValidationException("Autofill is only available for saved words.");
        }

        var generated = await enrichmentService.AutofillAsync(
            new VocabularyAutofillRequest(
                entry.Username,
                entry.Word,
                entry.Translation,
                string.IsNullOrWhiteSpace(entry.SourceLanguage) ? entry.TargetLanguage : entry.SourceLanguage,
                entry.TargetLanguage,
                entry.Examples.FirstOrDefault(example => example.IsFromBook)?.Text),
            ct);
        entry.WordDetails ??= new VocabularyWordDetailsEntity
        {
            VocabularyEntryId = entry.Id
        };

        if (string.IsNullOrWhiteSpace(entry.WordDetails.SeenForm))
        {
            entry.WordDetails.SeenForm = entry.Word;
        }

        if (string.IsNullOrWhiteSpace(entry.WordDetails.DictionaryForm)
            && !string.IsNullOrWhiteSpace(generated.DictionaryForm))
        {
            entry.WordDetails.DictionaryForm = generated.DictionaryForm.Trim();
        }

        if (!string.IsNullOrWhiteSpace(generated.DictionaryForm))
        {
            var normalizedDictionaryForm = generated.DictionaryForm.Trim();
            entry.WordDetails.DictionaryForm = normalizedDictionaryForm;
            entry.Word = normalizedDictionaryForm;
        }

        if (!string.IsNullOrWhiteSpace(generated.PrimaryTranslation))
        {
            entry.Translation = generated.PrimaryTranslation.Trim();
        }

        entry.WordDetails.Description = generated.Description;
        entry.WordDetails.FrequencyScore = generated.FrequencyScore;
        entry.WordDetails.PartOfSpeech = generated.PartOfSpeech;
        entry.WordDetails.Notes = generated.Notes;

        await dbContext.RelatedWords
            .Where(x => x.VocabularyEntryId == entry.Id)
            .ExecuteDeleteAsync(ct);

        var seeds = generated.AlternativeTranslations
            .Select(item => new VocabularyRelatedWordSeed(item, RelatedWordType.AlternativeTranslation))
            .Concat(generated.RelatedWords)
            .Where(item => !string.IsNullOrWhiteSpace(item.Word))
            .DistinctBy(item => (item.Word.Trim().ToLowerInvariant(), item.Type))
            .ToList();

        for (var index = 0; index < seeds.Count; index++)
        {
            var relatedWord = seeds[index];

            dbContext.RelatedWords.Add(new RelatedWordEntity
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

        await dbContext.SaveChangesAsync(ct);
        return entry.ToVocabularyEntryDto();
    }

    private async Task<VocabularyEntryEntity> LoadOwnedEntryAsync(Guid id, string username, CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(username);
        var entry = await dbContext.VocabularyEntries
            .Include(item => item.Book)
            .Include(item => item.WordDetails)
            .Include(item => item.RelatedWords)
            .Include(item => item.AiOperations)
            .Include(item => item.Examples)
                .ThenInclude(example => example.Book)
            .FirstOrDefaultAsync(item => item.Id == id && item.Username == normalizedUsername, ct);

        if (entry is null)
        {
            throw new NotFoundException($"Vocabulary entry '{id}' was not found.");
        }

        return entry;
    }
}
