using System.ComponentModel.DataAnnotations;
using LanguageReader.Infrastructure.Features.ReadingItems.Parsing;
using LanguageReader.Infrastructure.Features.ReadingItems.Parsing.Models;

namespace LanguageReader.Api.Features.ReadingItems;

internal sealed class ImportReadingItemHandler(
    IReadingItemContentParser parser,
    ReadingItemImportWriter importWriter)
{
    public async Task<ReadingItemDetailsDto> HandleAsync(ImportReadingItemRequest request, CancellationToken ct)
    {
        var username = UsernameHelper.Require(request.Username);
        var file = request.File;

        if (file is null || file.Length == 0)
        {
            throw new ValidationException("A source file is required.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension is not ".fb2" and not ".xml")
        {
            throw new ValidationException("Only .fb2 and .xml files are supported.");
        }

        var safeFileName = Path.GetFileName(file.FileName);

        ParsedReadingDocument parsedDocument;
        await using (var readStream = file.OpenReadStream())
        {
            parsedDocument = await parser.ParseAsync(readStream, ct);
        }

        return await importWriter.SaveBookAsync(
            username,
            request.Title,
            request.OriginalLanguage,
            safeFileName,
            parsedDocument,
            ct);
    }
}
