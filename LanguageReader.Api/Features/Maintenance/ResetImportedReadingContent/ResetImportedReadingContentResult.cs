namespace LanguageReader.Api.Features.Maintenance;

internal sealed record ResetImportedReadingContentResult(
    int RemovedReadingItems,
    int RemovedLegacyBooks,
    int ResetRssCandidates,
    int DeletedStorageFiles,
    IReadOnlyList<string> StorageDeleteFailures);
