using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Ai;
using LanguageReader.Infrastructure.Features.Vocabulary.Entities;
using LanguageReader.Infrastructure.Features.Vocabulary.Models.Enrichment;
using LanguageReader.Infrastructure.Features.Vocabulary.Services.Enrichment;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.Vocabulary;

internal sealed class AddVocabularyExampleHandler(
    ApplicationDbContext dbContext,
    IVocabularyEnrichmentService enrichmentService)
{
    public async Task<VocabularyEntryDto> HandleAsync(
        AddVocabularyExampleRequest request,
        CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);

        var entry = await dbContext.VocabularyEntries
            .Include(item => item.Examples)
            .FirstOrDefaultAsync(
                item => item.Id == request.VocabularyId && item.Username == normalizedUsername,
                ct);

        if (entry is null)
        {
            throw new NotFoundException($"Vocabulary entry '{request.VocabularyId}' was not found.");
        }

        if (entry.Kind != SavedTextKind.LexicalUnit)
        {
            throw new ValidationException("Generated usage examples are only available for saved words.");
        }

        var generated = await enrichmentService.GenerateExampleAsync(
            new VocabularyExampleGenerationRequest(
                entry.Username,
                entry.Word,
                entry.Translation,
                string.IsNullOrWhiteSpace(entry.SourceLanguage) ? entry.TargetLanguage : entry.SourceLanguage,
                entry.TargetLanguage,
                entry.Examples.FirstOrDefault(example => example.IsFromBook)?.Text),
            ct);

        dbContext.VocabularyExamples.Add(new VocabularyExampleEntity
        {
            Id = Guid.NewGuid(),
            VocabularyEntryId = entry.Id,
            Text = generated.Text,
            Translation = generated.Translation,
            IsFromBook = false,
            CreatedAtUtc = DateTimeOffset.UtcNow
        });

        dbContext.AiOperations.Add(AiOperationMapper.ToEntity(generated.Usage, normalizedUsername, vocabularyEntryId: entry.Id));
        await dbContext.SaveChangesAsync(ct);

        var updatedEntry = await dbContext.VocabularyEntries
            .Include(item => item.ReadingItem)
            .Include(item => item.WordDetails)
            .Include(item => item.RelatedWords)
            .Include(item => item.AiOperations)
            .Include(item => item.Examples)
                .ThenInclude(example => example.ReadingItem)
            .FirstAsync(
                item => item.Id == request.VocabularyId && item.Username == normalizedUsername,
                ct);

        return updatedEntry.ToVocabularyEntryDto();
    }
}
