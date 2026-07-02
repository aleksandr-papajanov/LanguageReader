namespace LanguageReader.Infrastructure.Features.Maintenance.Models;

public sealed record ResetImportedReadingContentSummary(
    int RemovedReadingItems,
    int RemovedLegacyBooks,
    int ResetNewsCandidates,
    int DeletedFiles,
    IReadOnlyList<string> DeleteFailures);
