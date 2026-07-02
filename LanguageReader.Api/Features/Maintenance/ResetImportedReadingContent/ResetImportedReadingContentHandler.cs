using System.ComponentModel.DataAnnotations;
using LanguageReader.Infrastructure.Features.Maintenance.Services;

namespace LanguageReader.Api.Features.Maintenance;

internal sealed class ResetImportedReadingContentHandler(ResetImportedReadingContentService resetImportedContent)
{
    public const string RequiredConfirmation = "reset-imported-content";

    public async Task<ResetImportedReadingContentResult> HandleAsync(
        ResetImportedReadingContentRequest request,
        CancellationToken ct)
    {
        if (!string.Equals(request.Confirmation, RequiredConfirmation, StringComparison.Ordinal))
        {
            throw new ValidationException($"Pass confirmation='{RequiredConfirmation}' to reset imported reading content.");
        }

        var result = await resetImportedContent.ResetAsync(ct);

        return new ResetImportedReadingContentResult(
            result.RemovedReadingItems,
            result.RemovedLegacyBooks,
            result.ResetNewsCandidates,
            result.DeletedFiles,
            result.DeleteFailures);
    }
}
