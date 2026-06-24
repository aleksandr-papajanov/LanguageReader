using LanguageReader.Api.Features.Maintenance;
using LanguageReader.Api.Features.ReadingItemTranslations;
using LanguageReader.Api.Features.ReadingItems;
using LanguageReader.Api.Features.News;
using LanguageReader.Api.Features.Reading;
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
        api.MapReadingItemsEndpoints();
        api.MapNewsEndpoints();
        api.MapReadingEndpoints();
        api.MapSettingsEndpoints();
        api.MapStatsEndpoints();
        api.MapTranslationEndpoints();
        api.MapVocabularyEndpoints();
        api.MapReadingItemTranslationsEndpoints();
        api.MapMaintenanceEndpoints();

        return app;
    }
}

