using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.Books;

internal sealed class UpdateBookVisibilityHandler(ApplicationDbContext dbContext)
{
    public async Task<BookDetailsDto> HandleAsync(UpdateBookVisibilityRequest request, CancellationToken ct)
    {
        var username = BookFeatureHelpers.RequireUsername(request.Username);
        var book = await dbContext.Books.FirstOrDefaultAsync(book => book.Id == request.BookId, ct);
        if (book is null)
        {
            throw new NotFoundException($"Book '{request.BookId}' was not found.");
        }

        if (book.OwnerUsername != username)
        {
            throw new ForbiddenException("You do not have access to this book.");
        }

        book.IsPublic = request.IsPublic;
        var readingItem = await dbContext.ReadingItems.FirstOrDefaultAsync(item => item.Id == request.BookId, ct);
        if (readingItem is not null)
        {
            readingItem.IsPublic = request.IsPublic;
            readingItem.UpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(ct);

        return book.ToBookDetailsDto();
    }
}
