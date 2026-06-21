namespace LanguageReader.Client.Features.Vocabulary.Services;

public sealed class VocabularyApiClient(ApiClient api)
{
    public Task<IReadOnlyList<VocabularyEntryDto>> GetVocabularyAsync(
        GetVocabularyRequest request,
        CancellationToken cancellationToken = default)
    {
        return api.GetAsync<IReadOnlyList<VocabularyEntryDto>>(
            $"/api/vocabulary",
            request,
            cancellationToken);
    }

    public Task<VocabularyEntryDto> GetVocabularyEntryAsync(
        GetVocabularyEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        return api.GetAsync<VocabularyEntryDto>(
            "/api/vocabulary/{VocabularyId}",
            request,
            cancellationToken);
    }

    public Task<VocabularyEntryDto> SaveVocabularyAsync(
        SaveVocabularyEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        return api.PostAsync<VocabularyEntryDto>(
            "/api/vocabulary",
            request,
            cancellationToken);
    }

    public Task<VocabularyEntryDto> AutofillVocabularyEntryAsync(
        AutofillVocabularyEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        var route = new AutofillVocabularyEntryRequestRoute(request.VocabularyId);
        var body = new AutofillVocabularyEntryRequestBody(request.Username);

        return api.PostAsync<VocabularyEntryDto>(
            "/api/vocabulary/{VocabularyId}/autofill",
            route,
            body,
            cancellationToken);
    }

    public Task<VocabularyEntryDto> AddVocabularyExampleAsync(
        AddVocabularyExampleRequest request,
        CancellationToken cancellationToken = default)
    {
        var route = new AddVocabularyExampleRequestRoute(request.VocabularyId);
        var body = new AddVocabularyExampleRequestBody(request.Username);

        return api.PostAsync<VocabularyEntryDto>(
            "/api/vocabulary/{VocabularyId}/examples",
            route,
            body,
            cancellationToken);
    }

    public Task DeleteVocabularyExampleAsync(
        DeleteVocabularyExampleRequest request,
        CancellationToken cancellationToken = default)
    {
        return api.DeleteAsync(
            "/api/vocabulary/{VocabularyId}/examples/{ExampleId}",
            request,
            cancellationToken);
    }

    public Task DeleteVocabularyEntryAsync(
        DeleteVocabularyEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        return api.DeleteAsync(
            "/api/vocabulary/{VocabularyId}",
            request,
            cancellationToken);
    }

    public Task<VocabularyEntryDto> UpdateVocabularyVisibilityAsync(
        UpdateVocabularyVisibilityRequest request,
        CancellationToken cancellationToken = default)
    {
        var route = new UpdateVocabularyVisibilityRequestRoute(request.VocabularyId);
        var body = new UpdateVocabularyVisibilityRequestBody(
            request.Username,
            request.IsVisibleInVocabulary);

        return api.PutAsync<VocabularyEntryDto>(
            "/api/vocabulary/{VocabularyId}/visibility",
            route,
            body,
            cancellationToken);
    }
}
