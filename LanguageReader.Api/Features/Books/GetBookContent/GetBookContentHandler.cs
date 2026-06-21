using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Books.Parsing;
using LanguageReader.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.Books;

internal sealed class GetBookContentHandler(
    ApplicationDbContext dbContext,
    IFileStorage storage,
    IBookContentParser parser)
{
    public async Task<BookContentDto> HandleAsync(GetBookContentRequest request, CancellationToken ct)
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

        await using var stream = await storage.OpenReadAsync(book.StoragePath, ct);
        var parsedBook = await parser.ParseAsync(stream, ct);

        return book.ToBookContentDto(parsedBook);
    }
}
