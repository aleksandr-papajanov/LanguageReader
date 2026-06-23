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
            parsedBook.Blocks.Select(x => new ReadingContentBlockDto(
                x.Type,
                x.Text,
                x.ImageId))
                .ToArray(),
            parsedBook.Images.ToDictionary(
                x => x.Key,
                x => new ReadingImageDto(
                    x.Value.Id,
                    x.Value.ContentType,
                    x.Value.Base64Content)));
    }

    private static async Task<ReadingItemContentDto> LoadArticleAsync(
        ReadingItemEntity item,
        Stream stream,
        CancellationToken cancellationToken)
    {
        var document = await JsonSerializer.DeserializeAsync<StoredArticleDocument>(
            stream,
            JsonOptions,
            cancellationToken)
            ?? throw new InvalidOperationException("Stored article content is invalid.");

        var blocks = document.Paragraphs
            .Where(paragraph => !string.IsNullOrWhiteSpace(paragraph))
            .Select(paragraph => new ReadingContentBlockDto(
                ReadingContentBlockType.Paragraph,
                paragraph.Trim(),
                ImageId: null))
            .ToArray();

        return new ReadingItemContentDto(
            item.Id,
            document.Title,
            item.Type,
            LanguageNameNormalizer.Normalize(item.OriginalLanguage),
            blocks,
            Images: new Dictionary<string, ReadingImageDto>());
    }
}
