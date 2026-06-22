using LanguageReader.Infrastructure.Agents.Json.Operations;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Translation.Operations;
using LanguageReader.Shared.Features.Settings;

namespace LanguageReader.Infrastructure.Features.Translation.Services;

public sealed class AiTranslationService(
    IAiJsonOperationRunner operationRunner) : ITranslationBackend
{
    public AiServiceMode Mode => AiServiceMode.Agent;

    public async Task<TranslationResultDto> TranslateAsync(
        TranslateRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await operationRunner.RunAsync(new TranslationAiJsonOperation(request), cancellationToken);

        if (string.IsNullOrWhiteSpace(result.Payload.TranslatedText))
        {
            throw new InfrastructureException("Translation provider returned empty translatedText.");
        }

        return request.ToTranslationResultDto(
            result.Payload.TranslatedText,
            result.Usage);
    }
}
