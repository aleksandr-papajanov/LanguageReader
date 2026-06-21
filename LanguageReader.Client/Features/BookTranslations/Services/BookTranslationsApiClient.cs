namespace LanguageReader.Client.Features.BookTranslations.Services;

public sealed class BookTranslationsApiClient(ApiClient api)
{
    public Task<IReadOnlyList<TranslatedRangeDto>> GetBookTranslationsAsync(
        GetBookTranslationsRequest request,
        CancellationToken cancellationToken = default)
    {
        return api.GetAsync<IReadOnlyList<TranslatedRangeDto>>(
            "/api/reading-items/{ReadingItemId}/translations",
            request,
            cancellationToken);
    }

    public Task<TranslatedRangeDto> CreateBookTranslationAsync(
        CreateTranslatedRangeRequest request,
        CancellationToken cancellationToken = default)
    {
        var route = new CreateTranslatedRangeRequestRoute(request.ReadingItemId);
        var body = new CreateTranslatedRangeRequestBody(
            request.OriginalText,
            request.TranslatedText,
            request.DictionaryForm,
            request.ResolvedSelectionKind,
            request.ParagraphIndex,
            request.StartOffset,
            request.EndOffset,
            request.Username,
            request.SelectionKind,
            request.Usage);

        return api.PostAsync<TranslatedRangeDto>(
            "/api/reading-items/{ReadingItemId}/translations",
            route,
            body,
            cancellationToken);
    }

    public Task<TranslatedRangeDto> UpdateBookTranslationDisplayAsync(
        UpdateTranslatedRangeDisplayRequest request,
        CancellationToken cancellationToken = default)
    {
        var route = new UpdateTranslatedRangeDisplayRequestRoute(request.ReadingItemId, request.TranslationId);
        var body = new UpdateTranslatedRangeDisplayRequestBody(request.Username, request.ShowOriginal);

        return api.PutAsync<TranslatedRangeDto>(
            "/api/reading-items/{ReadingItemId}/translations/{TranslationId}/display",
            route,
            body,
            cancellationToken);
    }

    public Task DeleteBookTranslationAsync(
        DeleteBookTranslationRequest request,
        CancellationToken cancellationToken = default)
    {
        return api.DeleteAsync(
            "/api/reading-items/{ReadingItemId}/translations/{TranslationId}",
            request,
            cancellationToken);
    }
}
