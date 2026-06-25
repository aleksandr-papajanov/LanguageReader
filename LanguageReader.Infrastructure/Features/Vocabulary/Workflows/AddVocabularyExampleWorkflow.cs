using LanguageReader.Infrastructure.Ai.Execution;
using LanguageReader.Infrastructure.Ai.Operations.Vocabulary;
using LanguageReader.Infrastructure.Ai.Workflows;
using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Ai;
using LanguageReader.Infrastructure.Features.Vocabulary.Entities;
using LanguageReader.Infrastructure.Features.Vocabulary.Models.Enrichment;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Infrastructure.Features.Vocabulary.Workflows;

public sealed class AddVocabularyExampleWorkflow(
    ApplicationDbContext dbContext)
    : IWorkflow<AddVocabularyExampleWorkflowRequest, VocabularyEntryEntity>
{
    public async Task<VocabularyEntryEntity> RunAsync(
        AddVocabularyExampleWorkflowRequest request,
        WorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var entry = request.Entry;
        var result = await context.ExecuteAsync(
            new VocabularyExamplesOperation(
                new VocabularyExampleGenerationRequest(
                    entry.Username,
                    entry.Word,
                    entry.Translation,
                    string.IsNullOrWhiteSpace(entry.SourceLanguage) ? entry.TargetLanguage : entry.SourceLanguage,
                    entry.TargetLanguage,
                    entry.Examples.FirstOrDefault(example => example.IsFromReadingItem)?.Text)),
            cancellationToken);

        var generated = BuildExampleResult(result);

        dbContext.VocabularyExamples.Add(new VocabularyExampleEntity
        {
            Id = Guid.NewGuid(),
            VocabularyEntryId = entry.Id,
            Text = generated.Text,
            Translation = generated.Translation,
            IsFromReadingItem = false,
            CreatedAtUtc = DateTimeOffset.UtcNow
        });

        dbContext.AiOperations.Add(AiOperationMapper.ToEntity(generated.Usage, entry.Username, vocabularyEntryId: entry.Id));
        await dbContext.SaveChangesAsync(cancellationToken);

        return await dbContext.VocabularyEntries
            .Include(item => item.ReadingItem)
            .Include(item => item.WordDetails)
            .Include(item => item.RelatedWords)
            .Include(item => item.AiOperations)
            .Include(item => item.Examples)
                .ThenInclude(example => example.ReadingItem)
            .FirstAsync(
                item => item.Id == entry.Id && item.Username == entry.Username,
                cancellationToken);
    }

    public static VocabularyGeneratedExampleResult BuildExampleResult(
        AiOperationExecutionResult<VocabularyExamplesOperation.Payload> result)
    {
        if (string.IsNullOrWhiteSpace(result.Payload.Text))
        {
            throw new InfrastructureException("Vocabulary example must include text.");
        }

        if (string.IsNullOrWhiteSpace(result.Payload.Translation))
        {
            throw new InfrastructureException("Vocabulary example must include translation.");
        }

        return new VocabularyGeneratedExampleResult(
            NormalizeRequiredText("text", result.Payload.Text),
            NormalizeRequiredText("translation", result.Payload.Translation),
            result.Usage);
    }

    private static string NormalizeRequiredText(string fieldName, string value)
    {
        var normalized = value.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InfrastructureException($"Vocabulary payload must include {fieldName}.");
        }

        return normalized;
    }
}

public sealed record AddVocabularyExampleWorkflowRequest(VocabularyEntryEntity Entry);
