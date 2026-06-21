using LanguageReader.Api.Features.BookTranslations;
using LanguageReader.Api.Features.Books;
using LanguageReader.Api.Features.News;
using LanguageReader.Api.Features.ReadingItems;
using LanguageReader.Api.Features.Settings.Services;
using LanguageReader.Api.Features.Reading;
using LanguageReader.Api.Features.Settings;
using LanguageReader.Api.Features.Translation;
using LanguageReader.Api.Features.Users;
using LanguageReader.Api.Features.Vocabulary;

namespace LanguageReader.Api.Features;

internal static class FeatureRegistration
{
    public static IServiceCollection AddFeatureHandlers(this IServiceCollection services)
    {
        services.AddScoped<UserSettingsAccessor>();

        services.AddScoped<GetBookHandler>();
        services.AddScoped<GetBookContentHandler>();
        services.AddScoped<CreateBookHandler>();
        services.AddScoped<UpdateBookVisibilityHandler>();
        services.AddScoped<DeleteBookHandler>();
        services.AddScoped<GetReadingItemsHandler>();
        services.AddScoped<GetReadingItemHandler>();
        services.AddScoped<GetReadingItemContentHandler>();
        services.AddScoped<UpdateReadingItemVisibilityHandler>();
        services.AddScoped<DeleteReadingItemHandler>();
        services.AddScoped<ImportNewsArticleHandler>();

        services.AddScoped<GetBookTranslationsHandler>();
        services.AddScoped<CreateBookTranslationHandler>();
        services.AddScoped<UpdateBookTranslationDisplayHandler>();
        services.AddScoped<DeleteBookTranslationHandler>();

        services.AddScoped<GetReadingProgressHandler>();
        services.AddScoped<SaveReadingProgressHandler>();

        services.AddScoped<GetUserSettingsHandler>();
        services.AddScoped<UpdateUserSettingsHandler>();
        services.AddScoped<TranslateSelectionHandler>();
        services.AddScoped<CreateSessionHandler>();

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

