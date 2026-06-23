using LanguageReader.Api.Features.Vocabulary.Services;
using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Vocabulary.Entities;
using LanguageReader.Infrastructure.Features.Vocabulary.Models.Enrichment;
using LanguageReader.Infrastructure.Features.Vocabulary.Services.Enrichment;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.Vocabulary;

internal sealed class AutofillVocabularyEntryHandler(
    ApplicationDbContext dbContext,
    IVocabularyEnrichmentService enrichmentService,
    VocabularyAutofillApplicator autofillApplicator)
{
    public async Task<VocabularyEntryDto> HandleAsync(AutofillVocabularyEntryRequest request, CancellationToken ct)
    {
        var entry = await LoadOwnedEntryAsync(request.VocabularyId, request.Username, ct);

        if (entry.Kind != SavedTextKind.LexicalUnit)
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
                entry.Examples.FirstOrDefault(example => example.IsFromReadingItem)?.Text),
            ct);
        autofillApplicator.Apply(entry, generated, ct);

        await dbContext.SaveChangesAsync(ct);
        return entry.ToVocabularyEntryDto();
    }

    private async Task<VocabularyEntryEntity> LoadOwnedEntryAsync(Guid id, string username, CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(username);
        var entry = await dbContext.VocabularyEntries
            .Include(item => item.ReadingItem)
            .Include(item => item.WordDetails)
            .Include(item => item.RelatedWords)
            .Include(item => item.AiOperations)
            .Include(item => item.Examples)
                .ThenInclude(example => example.ReadingItem)
            .FirstOrDefaultAsync(item => item.Id == id && item.Username == normalizedUsername, ct);

        if (entry is null)
        {
            throw new NotFoundException($"Vocabulary entry '{id}' was not found.");
        }

        return entry;
    }
}
