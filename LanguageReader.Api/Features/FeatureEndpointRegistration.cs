using LanguageReader.Api.Features.BookTranslations;
using LanguageReader.Api.Features.Books;
using LanguageReader.Api.Features.News;
using LanguageReader.Api.Features.Reading;
using LanguageReader.Api.Features.ReadingItems;
using LanguageReader.Api.Features.Settings;
using LanguageReader.Api.Features.Stats;
using LanguageReader.Api.Features.SystemInfo;
using LanguageReader.Api.Features.Translation;
using LanguageReader.Api.Features.Users;
using LanguageReader.Api.Features.Vocabulary;

namespace LanguageReader.Api.Features;

internal static class FeatureEndpointRegistration
{
    public static IEndpointRouteBuilder MapFeatureEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapSystemInfoEndpoints();

        var api = app.MapGroup("/api");
        api.MapUsersEndpoints();
        api.MapBooksEndpoints();
        api.MapReadingItemsEndpoints();
        api.MapNewsEndpoints();
        api.MapReadingEndpoints();
        api.MapSettingsEndpoints();
        api.MapStatsEndpoints();
        api.MapTranslationEndpoints();
        api.MapVocabularyEndpoints();
        api.MapBookTranslationsEndpoints();

        return app;
    }
}

