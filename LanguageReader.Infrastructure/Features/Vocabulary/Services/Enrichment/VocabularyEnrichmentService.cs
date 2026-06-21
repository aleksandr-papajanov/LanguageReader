using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Ai.Settings;
using LanguageReader.Infrastructure.Features.Vocabulary.Models.Enrichment;
using LanguageReader.Shared.Features.Settings;

namespace LanguageReader.Infrastructure.Features.Vocabulary.Services.Enrichment;

/// <summary>
/// Resolves the active vocabulary enrichment backend for the current user.
/// </summary>
public sealed class VocabularyEnrichmentService(
    IEnumerable<IVocabularyEnrichmentBackend> backends,
    IUserAiServiceModeResolver modeResolver) : IVocabularyEnrichmentService
{
    private readonly IReadOnlyDictionary<AiServiceMode, IVocabularyEnrichmentBackend> backendMap = backends.ToDictionary(item => item.Mode);

    public async Task<VocabularyNormalizationResult> NormalizeAsync(
        VocabularyNormalizationRequest request,
        CancellationToken cancellationToken = default)
    {
        var backend = await ResolveBackendAsync(request.Username, cancellationToken);
        return await backend.NormalizeAsync(request, cancellationToken);
    }

    public async Task<VocabularyAutofillResult> AutofillAsync(
        VocabularyAutofillRequest request,
        CancellationToken cancellationToken = default)
    {
        var backend = await ResolveBackendAsync(request.Username, cancellationToken);
        return await backend.AutofillAsync(request, cancellationToken);
    }

    public async Task<VocabularyGeneratedExampleResult> GenerateExampleAsync(
        VocabularyExampleGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var backend = await ResolveBackendAsync(request.Username, cancellationToken);
        return await backend.GenerateExampleAsync(request, cancellationToken);
    }

    private async Task<IVocabularyEnrichmentBackend> ResolveBackendAsync(string username, CancellationToken cancellationToken)
    {
        var mode = await modeResolver.ResolveAsync(username, cancellationToken);
        if (!backendMap.TryGetValue(mode, out var backend))
        {
            throw new InfrastructureException($"Vocabulary enrichment backend '{mode}' is not registered.");
        }

        return backend;
    }
}
