using System.Text.Json;
using LanguageReader.Infrastructure.Features.Books.Parsing;
using LanguageReader.Infrastructure.Features.Common.Language;
using LanguageReader.Infrastructure.Features.ReadingItems.Entities;
using LanguageReader.Infrastructure.Features.ReadingItems.Models;
using LanguageReader.Infrastructure.Storage;

namespace LanguageReader.Infrastructure.Features.ReadingItems.Services;

public sealed class ReadingItemContentService(
    IFileStorage storage,
    IBookContentParser bookContentParser) : IReadingItemContentService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<ReadingItemContentDto> LoadAsync(ReadingItemEntity item, CancellationToken cancellationToken = default)
    {
        await using var stream = await storage.OpenReadAsync(item.StoragePath, cancellationToken);

        return item.ContentFormat switch
        {
            ReadingContentFormat.Fb2 => await LoadBookAsync(item, stream, cancellationToken),
            ReadingContentFormat.ExtractedArticle => await LoadArticleAsync(item, stream, cancellationToken),
            _ => throw new InvalidOperationException($"Unsupported content format '{item.ContentFormat}'.")
        };
    }

    private async Task<ReadingItemContentDto> LoadBookAsync(
        ReadingItemEntity item,
        Stream stream,
        CancellationToken cancellationToken)
    {
        var parsedBook = await bookContentParser.ParseAsync(stream, cancellationToken);

        return new ReadingItemContentDto(
            item.Id,
            parsedBook.Title ?? item.Title,
            item.Type,
            LanguageNameNormalizer.Normalize(item.OriginalLanguage),
            parsedBook.Pages);
    }

    private static async Task<ReadingItemContentDto> LoadArticleAsync(
        ReadingItemEntity item,
        Stream stream,
        CancellationToken cancellationToken)
    {
        var document = await JsonSerializer.DeserializeAsync<StoredArticleDocument>(stream, JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Stored article content is invalid.");

        return new ReadingItemContentDto(
            item.Id,
            document.Title,
            item.Type,
            LanguageNameNormalizer.Normalize(item.OriginalLanguage),
            document.Paragraphs);
    }
}
