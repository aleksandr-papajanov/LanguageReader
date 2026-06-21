using LanguageReader.Infrastructure.Features.Books.Entities;
using LanguageReader.Infrastructure.Features.Books.Parsing;

namespace LanguageReader.Api.Features.Books;

internal static class BookMappingExtensions
{
    public static BookSummaryDto ToBookSummaryDto(this BookEntity book)
    {
        return new BookSummaryDto(
            book.Id,
            book.Title,
            string.Empty,
            string.Empty,
            book.OriginalLanguage,
            book.IsPublic,
            book.CreatedAtUtc);
    }

    public static BookDetailsDto ToBookDetailsDto(this BookEntity book)
    {
        return new BookDetailsDto(
            book.Id,
            book.Title,
            string.Empty,
            string.Empty,
            book.OriginalLanguage,
            book.IsPublic,
            book.CreatedAtUtc);
    }

    public static BookContentDto ToBookContentDto(this BookEntity book, ParsedBook parsedBook)
    {
        return new BookContentDto(
            book.Id,
            parsedBook.Title ?? book.Title,
            string.Empty,
            book.OriginalLanguage,
            parsedBook.Pages);
    }
}
