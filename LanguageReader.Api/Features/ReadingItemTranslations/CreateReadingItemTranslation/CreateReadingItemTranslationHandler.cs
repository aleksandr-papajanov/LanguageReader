using LanguageReader.Api.Features.ReadingItems;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.ReadingItemTranslations.Services;
using LanguageReader.Infrastructure.Features.ReadingItems.Entities;
using LanguageReader.Infrastructure.Features.ReadingItems.Services;

namespace LanguageReader.Api.Features.ReadingItemTranslations;

internal sealed class CreateReadingItemTranslationHandler(
    ReadingItemAccessService readingItems,
    ReadingItemTranslationService translations,
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

        var readingItem = await readingItems.LoadReadableReadOnlyAsync(request.ReadingItemId, normalizedUsername, ct);

        await ValidateRangeAsync(readingItem, request, normalizedUsername, ct);

        var kind = SavedTextKindMapper.FromSelectionKind(request.SelectionKind);
        var range = await translations.CreateAsync(request, normalizedUsername, kind, ct);

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
