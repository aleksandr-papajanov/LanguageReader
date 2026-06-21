using System.ComponentModel.DataAnnotations;
using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Features.Books.Entities;
using LanguageReader.Infrastructure.Features.Books.Parsing;
using LanguageReader.Infrastructure.Features.ReadingItems.Entities;
using LanguageReader.Infrastructure.Storage;

namespace LanguageReader.Api.Features.Books;

internal sealed class CreateBookHandler(
    ApplicationDbContext dbContext,
    IFileStorage storage,
    IBookContentParser parser)
{
    public async Task<BookDetailsDto> HandleAsync(CreateBookRequest request, CancellationToken ct)
    {
        var username = BookFeatureHelpers.RequireUsername(request.Username);
        var requestedTitle = request.Title?.Trim() ?? string.Empty;
        var originalLanguage = BookFeatureHelpers.NormalizeLanguage(request.OriginalLanguage);
        var file = request.File;

        if (file is null || file.Length == 0)
        {
            throw new ValidationException("Book file is required.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension is not ".fb2" and not ".xml")
        {
            throw new ValidationException("Only .fb2 and .xml files are supported.");
        }

        var bookId = Guid.NewGuid();
        var safeFileName = Path.GetFileName(file.FileName);
        var storagePath = Path.Combine("books", username, $"{bookId}{extension}");

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
            Id = bookId,
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
            Id = bookId,
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

        return book.ToBookDetailsDto();
    }
}
