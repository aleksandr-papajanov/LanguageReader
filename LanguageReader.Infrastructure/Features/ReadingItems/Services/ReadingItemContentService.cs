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

    public async Task<ReadingItemContentPageDto> LoadPageAsync(
        ReadingItemEntity item,
        GetReadingItemContentRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var stream = await storage.OpenReadAsync(item.StoragePath, cancellationToken);

        return item.ContentFormat switch
        {
            ReadingContentFormat.Fb2 => await LoadBookAsync(item, request, stream, cancellationToken),
            ReadingContentFormat.ExtractedArticle => await LoadArticleAsync(item, request, stream, cancellationToken),
            _ => throw new InvalidOperationException($"Unsupported content format '{item.ContentFormat}'.")
        };
    }

    private async Task<ReadingItemContentPageDto> LoadBookAsync(
        ReadingItemEntity item,
        GetReadingItemContentRequest request,
        Stream stream,
        CancellationToken cancellationToken)
    {
        var parsedBook = await bookContentParser.ParseAsync(stream, cancellationToken);
        var blocks = AssignAddressableBlockIndexes(parsedBook.Blocks.Select(x => new ReadingContentBlockDto(
            x.Type,
            x.Text,
            x.ImageId)))
            .ToArray();
        var page = ReadingItemContentPager.Slice(
            blocks,
            request.PageIndex,
            request.BlockIndex,
            request.TargetPageWeight);
        var pageImageIds = page.Blocks
            .Select(block => block.ImageId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.Ordinal);

        return new ReadingItemContentPageDto(
            item.Id,
            parsedBook.Title ?? item.Title,
            item.Type,
            LanguageNameNormalizer.Normalize(item.OriginalLanguage),
            page.PageIndex,
            page.TotalPages,
            page.StartBlockIndex,
            page.EndBlockIndex,
            page.TotalBlocks,
            page.Blocks,
            parsedBook.Images
                .Where(image => pageImageIds.Contains(image.Key))
                .ToDictionary(
                x => x.Key,
                x => new ReadingImageDto(
                    x.Value.Id,
                    x.Value.ContentType,
                    x.Value.Base64Content),
                StringComparer.OrdinalIgnoreCase));
    }

    private static async Task<ReadingItemContentPageDto> LoadArticleAsync(
        ReadingItemEntity item,
        GetReadingItemContentRequest request,
        Stream stream,
        CancellationToken cancellationToken)
    {
        var document = await JsonSerializer.DeserializeAsync<StoredArticleDocument>(
            stream,
            JsonOptions,
            cancellationToken)
            ?? throw new InvalidOperationException("Stored article content is invalid.");

        var blocks = AssignAddressableBlockIndexes(document.Paragraphs
            .Where(paragraph => !string.IsNullOrWhiteSpace(paragraph))
            .Select(paragraph => new ReadingContentBlockDto(
                ReadingContentBlockType.Paragraph,
                paragraph.Trim(),
                ImageId: null)))
            .ToArray();
        var page = ReadingItemContentPager.Slice(
            blocks,
            request.PageIndex,
            request.BlockIndex,
            request.TargetPageWeight);

        return new ReadingItemContentPageDto(
            item.Id,
            document.Title,
            item.Type,
            LanguageNameNormalizer.Normalize(item.OriginalLanguage),
            page.PageIndex,
            page.TotalPages,
            page.StartBlockIndex,
            page.EndBlockIndex,
            page.TotalBlocks,
            page.Blocks,
            Images: new Dictionary<string, ReadingImageDto>());
    }

    private static IEnumerable<ReadingContentBlockDto> AssignAddressableBlockIndexes(
        IEnumerable<ReadingContentBlockDto> blocks)
    {
        var blockIndex = 0;

        foreach (var block in blocks)
        {
            if (!IsAddressableTextBlock(block))
            {
                yield return block with { BlockIndex = null };
                continue;
            }

            yield return block with { BlockIndex = blockIndex };
            blockIndex++;
        }
    }

    private static bool IsAddressableTextBlock(ReadingContentBlockDto block)
    {
        return !string.IsNullOrWhiteSpace(block.Text)
            && block.Type is
                ReadingContentBlockType.Paragraph or
                ReadingContentBlockType.Heading1 or
                ReadingContentBlockType.Heading2 or
                ReadingContentBlockType.Quote or
                ReadingContentBlockType.Verse or
                ReadingContentBlockType.Author;
    }
}
