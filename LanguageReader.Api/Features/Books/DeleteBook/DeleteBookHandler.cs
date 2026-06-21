using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.Books;

internal sealed class DeleteBookHandler(
    ApplicationDbContext dbContext,
    IFileStorage storage)
{
    public async Task HandleAsync(DeleteBookRequest request, CancellationToken ct)
    {
        var normalizedUsername = BookFeatureHelpers.RequireUsername(request.Username);
        var book = await dbContext.Books.FirstOrDefaultAsync(book => book.Id == request.BookId, ct);
        if (book is null)
        {
            throw new NotFoundException($"Book '{request.BookId}' was not found.");
        }

        if (book.OwnerUsername != normalizedUsername)
        {
            throw new ForbiddenException("You do not have access to this book.");
        }

        var readingItem = await dbContext.ReadingItems.FirstOrDefaultAsync(item => item.Id == request.BookId, ct);
        dbContext.Books.Remove(book);
        if (readingItem is not null)
        {
            dbContext.ReadingItems.Remove(readingItem);
        }

        await dbContext.SaveChangesAsync(ct);
        await storage.DeleteAsync(book.StoragePath, ct);
    }
}

