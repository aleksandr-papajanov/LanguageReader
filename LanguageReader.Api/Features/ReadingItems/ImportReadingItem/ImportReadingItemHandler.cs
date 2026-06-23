using System.ComponentModel.DataAnnotations;
using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Features.Books.Entities;
using LanguageReader.Infrastructure.Features.Books.Parsing;
using LanguageReader.Infrastructure.Features.Common.Language;
using LanguageReader.Infrastructure.Features.ReadingItems.Entities;
using LanguageReader.Infrastructure.Storage;

namespace LanguageReader.Api.Features.ReadingItems;

internal sealed class ImportReadingItemHandler(
    ApplicationDbContext dbContext,
    IFileStorage storage,
    IBookContentParser parser)
{
    public async Task<ReadingItemDetailsDto> HandleAsync(ImportReadingItemRequest request, CancellationToken ct)
    {
        var username = UsernameHelper.Require(request.Username);
        var requestedTitle = request.Title?.Trim() ?? string.Empty;
        var originalLanguage = NormalizeLanguage(request.OriginalLanguage);
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

        var readingItemId = Guid.NewGuid();
        var safeFileName = Path.GetFileName(file.FileName);
        var storagePath = Path.Combine("books", username, $"{readingItemId}{extension}");

        await using (var uploadStream = file.OpenReadStream())
        {
            await storage.SaveAsync(storagePath, uploadStream, ct);
        }

        string? parsedTitle = null;
        await using (var readStream = await storage.OpenReadAsync(storagePath, ct))
        {
            var parsedBook = await parser.ParseAsync(readStream, ct);
            parsedTitle = parsedBook.Title;
        }

        var book = new BookEntity
        {
            Id = readingItemId,
            OwnerUsername = username,
            Title = string.IsNullOrWhiteSpace(requestedTitle)
                ? parsedTitle ?? Path.GetFileNameWithoutExtension(safeFileName)
                : requestedTitle,
            OriginalFileName = safeFileName,
            OriginalLanguage = originalLanguage,
            StoragePath = storagePath,
            IsPublic = false,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        var readingItem = new ReadingItemEntity
        {
            Id = readingItemId,
            OwnerUsername = username,
            Title = book.Title,
            OriginalLanguage = book.OriginalLanguage,
            StoragePath = storagePath,
            Type = ReadingItemType.Book,
            ContentFormat = ReadingContentFormat.Fb2,
            IsPublic = false,
            CreatedAtUtc = book.CreatedAtUtc,
            UpdatedAtUtc = book.CreatedAtUtc
        };

        dbContext.Books.Add(book);
        dbContext.ReadingItems.Add(readingItem);
        await dbContext.SaveChangesAsync(ct);

        return new ReadingItemDetailsDto(
            readingItem.Id,
            readingItem.Title,
            readingItem.Type,
            LanguageNameNormalizer.Normalize(readingItem.OriginalLanguage),
            readingItem.IsPublic,
            readingItem.CreatedAtUtc,
            readingItem.UpdatedAtUtc,
            SourceKey: null,
            SourceName: null,
            Author: null,
            PublishedAtUtc: null,
            OriginalUrl: null,
            ImageUrl: null,
            Excerpt: null,
            RssFeedUrl: null,
            ExternalId: null);
    }

    private static string NormalizeLanguage(string? language)
    {
        var normalized = language?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? "Unknown" : normalized;
    }
}
