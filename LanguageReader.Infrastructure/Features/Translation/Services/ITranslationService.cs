namespace LanguageReader.Infrastructure.Features.Translation.Services;

/// <summary>
/// Abstraction for translation and language analysis.
/// </summary>
public interface ITranslationService
{
    /// <summary>
    /// Translates or analyzes selected text.
    /// </summary>
    /// <param name="request">The translation request.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A translation result.</returns>
    Task<TranslationResultDto> TranslateAsync(TranslateRequest request, CancellationToken cancellationToken = default);
}

