using System.Security.Cryptography;
using System.Text;
using LanguageReader.Infrastructure.Features.ReadingItems.Entities;
using LanguageReader.Infrastructure.Features.ReadingItems.Parsing.Models;
using LanguageReader.Infrastructure.Storage;

namespace LanguageReader.Infrastructure.Features.ReadingItems.Services;

public sealed class ReadingItemDocumentStorageService(IFileStorage storage)
{
    public const int CurrentSchemaVersion = 1;

    public async Task<ReadingItemDocumentEntity> StoreAsync(
        ReadingItemEntity item,
        IReadOnlyList<ReadingContentBlockDto> sourceBlocks,
        IReadOnlyDictionary<string, ParsedReadingAsset> sourceAssets,
        string? coverImageId,
        DateTimeOffset createdAtUtc,
        CancellationToken cancellationToken)
    {
        var blocks = AssignAddressableBlockIndexes(sourceBlocks).ToArray();
        var pages = ReadingItemContentPager.BuildPagePlan(blocks);
        var assets = await StoreAssetsAsync(item, sourceAssets, coverImageId, cancellationToken);

        item.Blocks = blocks
            .Select((block, sequenceIndex) => new ReadingItemBlockEntity
            {
                ReadingItemId = item.Id,
                SequenceIndex = sequenceIndex,
                BlockIndex = block.BlockIndex,
                Type = block.Type,
                Text = block.Text,
                ImageId = block.ImageId,
                Weight = ReadingItemContentPager.CalculateBlockWeight(block)
            })
            .ToList();

        item.Assets = assets.ToList();

        var document = new ReadingItemDocumentEntity
        {
            ReadingItemId = item.Id,
            SchemaVersion = CurrentSchemaVersion,
            ContentHash = ComputeContentHash(blocks),
            TotalBlocks = blocks.Count(block => block.BlockIndex.HasValue),
            TotalPages = pages.Count,
            CreatedAtUtc = createdAtUtc
        };

        item.Document = document;

        return document;
    }

    private async Task<IReadOnlyList<ReadingItemAssetEntity>> StoreAssetsAsync(
        ReadingItemEntity item,
        IReadOnlyDictionary<string, ParsedReadingAsset> sourceAssets,
        string? coverImageId,
        CancellationToken cancellationToken)
    {
        var assets = new List<ReadingItemAssetEntity>();

        foreach (var image in sourceAssets.Values)
        {
            var fileName = GetAssetFileName(image.ContentType, image.Id);
            var storagePath = Path.Combine("reading-items", item.OwnerUsername, item.Id.ToString(), "assets", fileName);
            var bytes = Convert.FromBase64String(image.Base64Content);

            await using var stream = new MemoryStream(bytes);
            await storage.SaveAsync(storagePath, stream, cancellationToken);

            assets.Add(new ReadingItemAssetEntity
            {
                ReadingItemId = item.Id,
                AssetId = image.Id,
                Kind = "Image",
                ContentType = image.ContentType,
                StoragePath = storagePath,
                IsCover = !string.IsNullOrWhiteSpace(coverImageId)
                    && string.Equals(image.Id, coverImageId, StringComparison.OrdinalIgnoreCase)
            });
        }

        return assets;
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

    private static string ComputeContentHash(IReadOnlyList<ReadingContentBlockDto> blocks)
    {
        using var sha = SHA256.Create();
        var text = string.Join(
            '\n',
            blocks.Select(block => $"{block.Type}|{block.BlockIndex}|{block.ImageId}|{block.Text}"));
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(text));

        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string SanitizePathSegment(string value)
    {
        var builder = new StringBuilder(value.Length);

        foreach (var character in value)
        {
            builder.Append(char.IsLetterOrDigit(character) || character is '-' or '_' or '.'
                ? character
                : '-');
        }

        return builder.Length == 0 ? Guid.NewGuid().ToString("N") : builder.ToString();
    }

    private static string GetAssetFileName(string contentType, string imageId)
    {
        var safeId = SanitizePathSegment(imageId);
        if (!string.IsNullOrWhiteSpace(Path.GetExtension(safeId)))
        {
            return safeId;
        }

        var extension = contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            "image/webp" => ".webp",
            "image/svg+xml" => ".svg",
            _ => ".bin"
        };

        return $"{safeId}{extension}";
    }
}
