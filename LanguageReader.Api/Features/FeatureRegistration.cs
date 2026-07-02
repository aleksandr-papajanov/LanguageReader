using LanguageReader.Api.Features.Maintenance;
using LanguageReader.Api.Features.ReadingItemTranslations;
using LanguageReader.Api.Features.ReadingItems;
using LanguageReader.Api.Features.News;
using LanguageReader.Api.Features.Reading;
using LanguageReader.Api.Features.Settings;
using LanguageReader.Api.Features.Stats;
using LanguageReader.Api.Features.Translation;
using LanguageReader.Api.Features.Users;
using LanguageReader.Api.Features.Vocabulary;

namespace LanguageReader.Api.Features;

internal static class FeatureRegistration
{
    public static IServiceCollection AddFeatureHandlers(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ReadingItemApiUrlBuilder>();
        services.AddScoped<ReadingItemImportWriter>();

        services.AddScoped<ImportReadingItemHandler>();
        services.AddScoped<PreviewReadingItemUrlImportHandler>();
        services.AddScoped<ImportReadingItemFromUrlHandler>();
        services.AddScoped<GetReadingItemsHandler>();
        services.AddScoped<GetReadingItemHandler>();
        services.AddScoped<GetReadingItemContentHandler>();
        services.AddScoped<GetReadingItemAssetHandler>();
        services.AddScoped<GetRemoteImageProxyHandler>();
        services.AddScoped<UpdateReadingItemVisibilityHandler>();
        services.AddScoped<DeleteReadingItemHandler>();
        services.AddScoped<ResetImportedReadingContentHandler>();
        services.AddScoped<PreviewNewsArticleHandler>();
        services.AddScoped<ImportNewsArticleHandler>();

        services.AddScoped<GetReadingItemTranslationsHandler>();
        services.AddScoped<CreateReadingItemTranslationHandler>();
        services.AddScoped<UpdateReadingItemTranslationDisplayHandler>();
        services.AddScoped<DeleteReadingItemTranslationHandler>();

        services.AddScoped<GetReadingProgressHandler>();
        services.AddScoped<SaveReadingProgressHandler>();

        services.AddScoped<GetUserSettingsHandler>();
        services.AddScoped<UpdateUserSettingsHandler>();
        services.AddScoped<GetAiOperationsStatsHandler>();
        services.AddScoped<TranslateSelectionHandler>();
        services.AddScoped<PasswordHashService>();
        services.AddScoped<CreateSessionHandler>();
        services.AddScoped<RegisterUserHandler>();

        services.AddScoped<GetVocabularyHandler>();
        services.AddScoped<GetVocabularyEntryHandler>();
        services.AddScoped<SaveVocabularyEntryHandler>();
        services.AddScoped<DeleteVocabularyEntryHandler>();
        services.AddScoped<AutofillVocabularyEntryHandler>();
        services.AddScoped<AddVocabularyExampleHandler>();
        services.AddScoped<DeleteVocabularyExampleHandler>();
        services.AddScoped<UpdateVocabularyVisibilityHandler>();

        return services;
    }
}
