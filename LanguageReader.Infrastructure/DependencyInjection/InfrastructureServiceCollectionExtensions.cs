using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Storage;
using LanguageReader.Shared.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LanguageReader.Infrastructure.Ai.Execution;
using LanguageReader.Infrastructure.Ai.Operations.Translation.Tools;
using LanguageReader.Infrastructure.Ai.Providers;
using LanguageReader.Infrastructure.Ai.Providers.OpenAI;
using LanguageReader.Infrastructure.Ai.Workflows;
using LanguageReader.Infrastructure.Features.Common.Language;
using LanguageReader.Infrastructure.Features.Maintenance.Services;
using LanguageReader.Infrastructure.Features.News.Services;
using LanguageReader.Infrastructure.Features.Reading.Services;
using LanguageReader.Infrastructure.Features.ReadingItemTranslations.Services;
using LanguageReader.Infrastructure.Features.ReadingItems.Parsing;
using LanguageReader.Infrastructure.Features.ReadingItems.Services;
using LanguageReader.Infrastructure.Features.Settings.Services;
using LanguageReader.Infrastructure.Features.Stats.Services;
using LanguageReader.Infrastructure.Features.Translation.Workflows;
using LanguageReader.Infrastructure.Features.Users.Services;
using LanguageReader.Infrastructure.Features.Vocabulary.Services;
using LanguageReader.Infrastructure.Features.Vocabulary.Workflows;
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
        services.AddSingleton<IReadingItemContentParser, Fb2ReadingItemContentParser>();
        services.AddScoped<ReadingItemDocumentStorageService>();
        services.AddScoped<ReadingItemAccessService>();
        services.AddScoped<ReadingItemAssetService>();
        services.AddScoped<ReadingItemDeletionService>();
        services.AddScoped<ReadingItemImportService>();
        services.AddScoped<ReadingItemLibraryQueryService>();
        services.AddScoped<ReadingItemDiscoveryService>();
        services.AddScoped<ReadingItemVisibilityService>();
        services.AddScoped<IReadingItemContentService, ReadingItemContentService>();
        services.AddScoped<ReadingItemTranslationService>();
        services.AddScoped<ReadingProgressService>();
        services.AddScoped<AiOperationsStatsService>();
        services.AddScoped<UserAccountService>();
        services.AddScoped<UserSettingsService>();
        services.AddScoped<NewsArticleImportService>();
        services.AddScoped<ResetImportedReadingContentService>();
        services.AddScoped<IAiModelResolver, AiModelResolver>();
        services.AddScoped<IAiJsonRequestService, AiJsonRequestService>();
        services.AddScoped<IAiExecutor, AiExecutor>();
        services.AddScoped<IAiExecutionHandler, AgentHandler>();
        services.AddScoped<IAiExecutionHandler, SinglePromptHandler>();
        services.AddScoped<IAiExecutionHandler, ConversationHandler>();
        services.AddScoped<WorkflowRunner>();
        services.AddScoped<TranslationContextTool>();
        services.AddScoped<TranslateSelectionWorkflow>();
        services.AddScoped<SaveVocabularyEntryWorkflow>();
        services.AddScoped<AddVocabularyExampleWorkflow>();
        services.AddScoped<VocabularyAutofillApplicator>();
        services.AddScoped<VocabularyEntryDeletionService>();
        services.AddScoped<VocabularyEntryGraphService>();
        services.AddScoped<VocabularyEntrySaveService>();
        services.AddScoped<VocabularyExampleDeletionService>();
        services.AddScoped<VocabularyTranslatedRangeService>();
        services.AddScoped<VocabularyVisibilityService>();
        services.AddSingleton<IVocabularyNormalizationRuleProvider, VocabularyNormalizationRuleProvider>();
        services.AddHttpClient<INewsFeedService, NewsFeedService>(ConfigureContentHttpClient);
        services.AddHttpClient<IArticleImportService, ArticleImportService>(ConfigureContentHttpClient);
        services.AddHttpClient<RemoteImageProxyService>();

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
