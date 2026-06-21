using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.Books;

internal sealed class GetBookHandler(ApplicationDbContext dbContext)
{
    public async Task<BookDetailsDto> HandleAsync(GetBookRequest request, CancellationToken ct)
    {
        var book = await dbContext.Books.AsNoTracking().FirstOrDefaultAsync(book => book.Id == request.BookId, ct);
        if (book is null)
        {
            throw new NotFoundException($"Book '{request.BookId}' was not found.");
        }

        if (!BookFeatureHelpers.CanRead(book, BookFeatureHelpers.NormalizeUsername(request.Username)))
        {
            throw new ForbiddenException("You do not have access to this book.");
        }

        return book.ToBookDetailsDto();
    }
}
