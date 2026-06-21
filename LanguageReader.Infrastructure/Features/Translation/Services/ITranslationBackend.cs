using LanguageReader.Shared.Features.Settings;

namespace LanguageReader.Infrastructure.Features.Translation.Services;

public interface ITranslationBackend
{
    AiServiceMode Mode { get; }

    Task<TranslationResultDto> TranslateAsync(TranslateRequest request, CancellationToken cancellationToken = default);
}
