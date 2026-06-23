using LanguageReader.Api.Features.ReadingItems;
using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Ai;
using LanguageReader.Infrastructure.Features.ReadingItemTranslations.Entities;
using LanguageReader.Infrastructure.Features.ReadingItems.Entities;
using LanguageReader.Infrastructure.Features.ReadingItems.Services;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.ReadingItemTranslations;

internal sealed class CreateReadingItemTranslationHandler(
    ApplicationDbContext dbContext,
    IReadingItemContentService readingItemContentService)
{
    public async Task<TranslatedRangeDto> HandleAsync(CreateTranslatedRangeRequest request, CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);

        if (request.BlockIndex < 0 || request.StartOffset < 0 || request.EndOffset <= request.StartOffset)
        {
            throw new ValidationException("Translation range is invalid.");
        }

        if (string.IsNullOrWhiteSpace(request.OriginalText) || string.IsNullOrWhiteSpace(request.TranslatedText))
        {
            throw new ValidationException("Original and translated text are required.");
        }

        var readingItem = await dbContext.ReadingItems.AsNoTracking().FirstOrDefaultAsync(item => item.Id == request.ReadingItemId, ct);
        if (readingItem is null)
        {
            throw new NotFoundException($"Reading item '{request.ReadingItemId}' was not found.");
        }

        if (!ReadingItemFeatureHelpers.CanRead(readingItem, normalizedUsername))
        {
            throw new ForbiddenException("You do not have access to this reading item.");
        }

        await ValidateRangeAsync(readingItem, request, normalizedUsername, ct);

        var kind = SavedTextKindMapper.FromSelectionKind(request.SelectionKind);
        var range = new TranslatedRangeEntity
        {
            Id = Guid.NewGuid(),
            Username = normalizedUsername,
            ReadingItemId = request.ReadingItemId,
            BlockIndex = request.BlockIndex,
            StartOffset = request.StartOffset,
            EndOffset = request.EndOffset,
            OriginalText = request.OriginalText.Trim(),
            TranslatedText = request.TranslatedText.Trim(),
            ShowOriginal = false,
            Kind = kind,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        dbContext.TranslatedRanges.Add(range);
        if (request.Usage is not null)
        {
            dbContext.AiOperations.Add(AiOperationMapper.ToEntity(
                request.Usage,
                normalizedUsername,
                translatedRangeId: range.Id));
        }

        await dbContext.SaveChangesAsync(ct);

        return range.ToTranslatedRangeDto();
    }

    private async Task ValidateRangeAsync(
        ReadingItemEntity readingItem,
        CreateTranslatedRangeRequest request,
        string normalizedUsername,
        CancellationToken ct)
    {
        var contentPage = await readingItemContentService.LoadPageAsync(
            readingItem,
            new GetReadingItemContentRequest(
                readingItem.Id,
                normalizedUsername,
                BlockIndex: request.BlockIndex),
            ct);

        var block = contentPage.Blocks.FirstOrDefault(candidate => candidate.BlockIndex == request.BlockIndex);
        if (block is null || string.IsNullOrWhiteSpace(block.Text))
        {
            throw new ValidationException("Translation range must target a readable text block.");
        }

        if (request.EndOffset > block.Text.Length)
        {
            throw new ValidationException("Translation range offsets are outside the target text block.");
        }

        var selectedText = block.Text[request.StartOffset..request.EndOffset].Trim();
        if (!string.Equals(selectedText, request.OriginalText.Trim(), StringComparison.Ordinal))
        {
            throw new ValidationException("Translation range text does not match the target reading item content.");
        }
    }
}
