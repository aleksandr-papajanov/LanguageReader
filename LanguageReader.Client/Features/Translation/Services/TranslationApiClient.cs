namespace LanguageReader.Client.Features.Translation.Services;

public sealed class TranslationApiClient(ApiClient api)
{
    public Task<TranslationResultDto> TranslateAsync(
        TranslateRequest request,
        CancellationToken cancellationToken = default)
    {
        return api.PostAsync<TranslationResultDto>(
            "/api/translation",
            body: request,
            ct: cancellationToken);
    }
}
