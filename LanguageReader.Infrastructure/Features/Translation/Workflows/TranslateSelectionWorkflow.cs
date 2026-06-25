using LanguageReader.Infrastructure.Ai.Operations.Translation;
using LanguageReader.Infrastructure.Ai.Operations.Translation.Tools;
using LanguageReader.Infrastructure.Ai.Workflows;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Translation.Mapping;

namespace LanguageReader.Infrastructure.Features.Translation.Workflows;

public sealed class TranslateSelectionWorkflow(
    TranslationContextTool contextTool) : IWorkflow<TranslateRequest, TranslationResultDto>
{
    public async Task<TranslationResultDto> RunAsync(
        TranslateRequest request,
        WorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var result = await context.ExecuteAsync(
            new ContextualTranslationOperation(request, contextTool),
            cancellationToken);

        if (string.IsNullOrWhiteSpace(result.Payload.TranslatedText))
        {
            throw new InfrastructureException("Translation provider returned empty translatedText.");
        }

        return request.ToTranslationResultDto(
            result.Payload.TranslatedText,
            result.Usage);
    }
}
