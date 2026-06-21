using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Ai.Settings;
using LanguageReader.Shared.Features.Settings;

namespace LanguageReader.Infrastructure.Features.Translation.Services;

/// <summary>
/// Resolves the active translation backend for the current user.
/// </summary>
public sealed class TranslationService(
    IEnumerable<ITranslationBackend> backends,
    IUserAiServiceModeResolver modeResolver) : ITranslationService
{
    private readonly IReadOnlyDictionary<AiServiceMode, ITranslationBackend> backendMap = backends.ToDictionary(item => item.Mode);

    public async Task<TranslationResultDto> TranslateAsync(TranslateRequest request, CancellationToken cancellationToken = default)
    {
        var mode = await modeResolver.ResolveAsync(request.Username, cancellationToken);
        if (!backendMap.TryGetValue(mode, out var backend))
        {
            throw new InfrastructureException($"Translation backend '{mode}' is not registered.");
        }

        return await backend.TranslateAsync(request, cancellationToken);
    }
}
