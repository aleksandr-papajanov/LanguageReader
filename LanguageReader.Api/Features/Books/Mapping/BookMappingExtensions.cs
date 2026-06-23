using LanguageReader.Infrastructure.Features.Books.Entities;
using LanguageReader.Infrastructure.Features.Books.Parsing;
using LanguageReader.Infrastructure.Features.Books.Parsing.Models;

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

    public static BookContentDto ToBookContentDto(
        this BookEntity book,
        ParsedBook parsedBook)
    {
        return new BookContentDto(
            book.Id,
            parsedBook.Title ?? book.Title,
            string.Empty,
            book.OriginalLanguage,
            parsedBook.Blocks.Select(ToBookBlockDto).ToArray(),
            parsedBook.Images.ToDictionary(
                image => image.Key,
                image => image.Value.ToBookImageDto()));
    }

    private static BookContentBlockDto ToBookBlockDto(ParsedBookBlock block)
    {
        return new BookContentBlockDto(
            ToBookBlockType(block.Type),
            block.Text,
            block.ImageId);
    }

    private static BookImageDto ToBookImageDto(this ParsedBookImage image)
    {
        return new BookImageDto(
            image.Id,
            image.ContentType,
            image.Base64Content);
    }

    private static BookBlockType ToBookBlockType(BookBlockType type)
    {
        return type;
    }
}
