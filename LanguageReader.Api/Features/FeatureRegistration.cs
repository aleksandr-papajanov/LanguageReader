using LanguageReader.Api.Features.ReadingItemTranslations;
using LanguageReader.Api.Features.ReadingItems;
using LanguageReader.Api.Features.News;
using LanguageReader.Api.Features.Reading;
using LanguageReader.Api.Features.Settings;
using LanguageReader.Api.Features.Stats;
using LanguageReader.Api.Features.Translation;
using LanguageReader.Api.Features.Users;
using LanguageReader.Api.Features.Vocabulary;
using LanguageReader.Api.Features.Vocabulary.Services;

namespace LanguageReader.Api.Features;

internal static class FeatureRegistration
{
    public static IServiceCollection AddFeatureHandlers(this IServiceCollection services)
    {
        services.AddScoped<UserSettingsAccessor>();

        services.AddScoped<ImportReadingItemHandler>();
        services.AddScoped<GetReadingItemsHandler>();
        services.AddScoped<GetReadingItemHandler>();
        services.AddScoped<GetReadingItemContentHandler>();
        services.AddScoped<UpdateReadingItemVisibilityHandler>();
        services.AddScoped<DeleteReadingItemHandler>();
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
        services.AddScoped<VocabularyAutofillApplicator>();
        services.AddScoped<SaveVocabularyEntryHandler>();
        services.AddScoped<DeleteVocabularyEntryHandler>();
        services.AddScoped<AutofillVocabularyEntryHandler>();
        services.AddScoped<AddVocabularyExampleHandler>();
        services.AddScoped<DeleteVocabularyExampleHandler>();
        services.AddScoped<UpdateVocabularyVisibilityHandler>();

        return services;
    }
}
