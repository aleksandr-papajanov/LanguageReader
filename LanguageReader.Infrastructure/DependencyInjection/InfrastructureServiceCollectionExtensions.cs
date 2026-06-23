using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Storage;
using LanguageReader.Shared.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LanguageReader.Infrastructure.Agents.Core;
using LanguageReader.Infrastructure.Agents.Json;
using LanguageReader.Infrastructure.Agents.Json.Operations;
using LanguageReader.Infrastructure.Agents.Tools;
using LanguageReader.Infrastructure.Agents.Providers;
using LanguageReader.Infrastructure.Agents.Prompts;
using LanguageReader.Infrastructure.Agents.Providers.OpenAI;
using LanguageReader.Infrastructure.Features.Books.Parsing;
using LanguageReader.Infrastructure.Features.Common.Language;
using LanguageReader.Infrastructure.Features.News.Services;
using LanguageReader.Infrastructure.Features.ReadingItems.Services;
using LanguageReader.Infrastructure.Features.Translation.Services;
using LanguageReader.Infrastructure.Features.Vocabulary.Services.Enrichment;
using LanguageReader.Infrastructure.Features.Ai.Settings;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace LanguageReader.Infrastructure.DependencyInjection;

/// <summary>
/// Dependency injection registration methods for infrastructure concerns.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    private const string BrowserUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/137.0.0.0 Safari/537.36";

    /// <summary>
    /// Registers all infrastructure services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabase(configuration);
        services.AddStorage(configuration);
        services.AddAiProviders(configuration);
        services.AddApplicationServices(configuration);

        return services;
    }

    /// <summary>
    /// Registers Entity Framework Core and PostgreSQL services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

            options.UseNpgsql(
                databaseOptions.ConnectionString,
                npgsqlOptions => 
                {
                    npgsqlOptions.CommandTimeout(databaseOptions.CommandTimeoutSeconds);
                    npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                });
            
            options.ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
        });

        return services;
    }

    /// <summary>
    /// Registers file storage abstractions and implementations.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));
        services.AddSingleton<IFileStorage>(serviceProvider =>
        {
            var storageOptions = serviceProvider.GetRequiredService<IOptions<StorageOptions>>().Value;
            return storageOptions.Provider.Trim().ToLowerInvariant() switch
            {
                "local" => ActivatorUtilities.CreateInstance<LocalFileStorage>(serviceProvider),
                "supabase" or "s3" => ActivatorUtilities.CreateInstance<SupabaseS3FileStorage>(serviceProvider),
                _ => throw new InvalidOperationException($"Unsupported storage provider '{storageOptions.Provider}'.")
            };
        });

        return services;
    }

    /// <summary>
    /// Registers application service foundations and future external HTTP clients.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OpenAiOptions>(configuration.GetSection(OpenAiOptions.SectionName));
        services.AddSingleton<IBookContentParser, Fb2BookContentParser>();
        services.AddScoped<IReadingItemContentService, ReadingItemContentService>();
        services.AddScoped<IUserAiServiceModeResolver, UserAiServiceModeResolver>();
        services.AddScoped<IAiModelResolver, AiModelResolver>();
        services.AddScoped<IAiJsonRequestService, AiJsonRequestService>();
        services.AddScoped<IAiJsonOperationRunner, AiJsonOperationRunner>();
        services.AddSingleton<IVocabularyNormalizationRuleProvider, VocabularyNormalizationRuleProvider>();
        services.AddScoped<ITranslationService, TranslationService>();
        services.AddScoped<ITranslationBackend, FakeTranslationService>();
        services.AddScoped<ITranslationBackend, AiTranslationService>();
        services.AddScoped<IVocabularyEnrichmentService, VocabularyEnrichmentService>();
        services.AddScoped<IVocabularyEnrichmentBackend, FakeVocabularyEnrichmentService>();
        services.AddScoped<IVocabularyEnrichmentBackend, AiVocabularyEnrichmentService>();
        services.AddHttpClient<INewsFeedService, NewsFeedService>(ConfigureContentHttpClient);
        services.AddHttpClient<IArticleImportService, ArticleImportService>(ConfigureContentHttpClient);

        return services;
    }

    /// <summary>
    /// Registers legacy provider-neutral agent services kept for future experimentation.
    /// This path is not part of the active direct-JSON runtime flow.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddLegacyAgentServices(this IServiceCollection services)
    {
        services.AddSingleton<IToolDispatcher, ToolDispatcher>();
        services.AddTransient<IAgentFactory, AgentFactory>();
        services.AddSingleton<IAgentPromptStore, InMemoryAgentPromptStore>();

        return services;
    }

    /// <summary>
    /// Registers concrete AI provider clients.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddAiProviders(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OpenAiOptions>(configuration.GetSection(OpenAiOptions.SectionName));

        services.AddHttpClient<OpenAiResponsesClient>((serviceProvider, client) =>
        {
            var openAiOptions = serviceProvider.GetRequiredService<IOptions<OpenAiOptions>>().Value;

            var baseUrl = openAiOptions.BaseUrl.EndsWith("/", StringComparison.Ordinal)
                ? openAiOptions.BaseUrl
                : openAiOptions.BaseUrl + "/";

            if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
            {
                client.BaseAddress = baseUri;
            }
        });

        services.AddTransient<IAiProviderClient>(serviceProvider =>
            serviceProvider.GetRequiredService<OpenAiResponsesClient>());

        return services;
    }

    private static void ConfigureContentHttpClient(HttpClient client)
    {
        client.DefaultRequestHeaders.UserAgent.ParseAdd(BrowserUserAgent);
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("sv-SE,sv;q=0.9,en;q=0.8");
    }
}
