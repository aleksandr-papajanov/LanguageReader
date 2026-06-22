using LanguageReader.Api.Features.Common.Services;
using LanguageReader.Api.Features.ReadingItems;
using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Ai;
using LanguageReader.Infrastructure.Features.BookTranslations.Entities;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.BookTranslations;

internal sealed class CreateBookTranslationHandler(ApplicationDbContext dbContext)
{
    public async Task<TranslatedRangeDto> HandleAsync(CreateTranslatedRangeRequest request, CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);

        if (request.ParagraphIndex < 0 || request.StartOffset < 0 || request.EndOffset <= request.StartOffset)
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

        var kind = SavedTextKindMapper.FromSelectionKind(request.SelectionKind);
        var range = new TranslatedRangeEntity
        {
            Id = Guid.NewGuid(),
            Username = normalizedUsername,
            ReadingItemId = request.ReadingItemId,
            ParagraphIndex = request.ParagraphIndex,
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
}
