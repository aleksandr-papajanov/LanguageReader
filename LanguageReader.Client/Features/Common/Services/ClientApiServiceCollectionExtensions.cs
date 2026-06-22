namespace LanguageReader.Client.Features.Common.Services;

public static class ClientApiServiceCollectionExtensions
{
    public static IServiceCollection AddLanguageReaderApiClients(this IServiceCollection services)
    {
        services.AddScoped<ApiClient>();
        services.AddScoped<ThemeService>();
        services.AddScoped<SessionApiClient>();
        services.AddScoped<BooksApiClient>();
        services.AddScoped<ReadingItemsApiClient>();
        services.AddScoped<NewsApiClient>();
        services.AddScoped<ReadingApiClient>();
        services.AddScoped<SettingsApiClient>();
        services.AddScoped<StatsApiClient>();
        services.AddScoped<TranslationApiClient>();
        services.AddScoped<BookTranslationsApiClient>();
        services.AddScoped<VocabularyApiClient>();
        services.AddScoped<ReaderSessionCache>();

        return services;
    }
}
