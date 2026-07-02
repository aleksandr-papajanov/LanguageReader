namespace LanguageReader.Infrastructure.Features.Ai.Models;

internal static class AiPricingCatalog
{
    public static (decimal InputUsdPerMillionTokens, decimal OutputUsdPerMillionTokens) GetPricing(
        string provider,
        string model)
    {
        if (string.Equals(provider, "OpenAI", StringComparison.OrdinalIgnoreCase))
        {
            if (model.Contains("gpt-5-nano", StringComparison.OrdinalIgnoreCase))
            {
                return (0.05m, 0.40m);
            }

            if (model.Contains("gpt-5-mini", StringComparison.OrdinalIgnoreCase))
            {
                return (0.25m, 2.00m);
            }

            if (model.Contains("gpt-5", StringComparison.OrdinalIgnoreCase))
            {
                return (1.25m, 10.00m);
            }

            if (model.Contains("gpt-4.1-mini", StringComparison.OrdinalIgnoreCase))
            {
                return (0.40m, 1.60m);
            }

            if (model.Contains("gpt-4.1", StringComparison.OrdinalIgnoreCase))
            {
                return (2.00m, 8.00m);
            }
        }

        if (string.Equals(provider, "OpenRouter", StringComparison.OrdinalIgnoreCase))
        {
            if (model.Contains(":free", StringComparison.OrdinalIgnoreCase)
                || model.Equals("openrouter/free", StringComparison.OrdinalIgnoreCase))
            {
                return (0.00m, 0.00m);
            }

            if (model.Contains("gpt-5-nano", StringComparison.OrdinalIgnoreCase))
            {
                return (0.05m, 0.40m);
            }

            if (model.Contains("deepseek-v4-flash", StringComparison.OrdinalIgnoreCase))
            {
                return (0.089m, 0.18m);
            }

            if (model.Equals("qwen/qwen-plus", StringComparison.OrdinalIgnoreCase))
            {
                return (0.26m, 0.78m);
            }
        }

        return (0.0m, 0.0m);
    }
}
