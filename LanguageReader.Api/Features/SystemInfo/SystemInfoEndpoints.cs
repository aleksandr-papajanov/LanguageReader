using System.Reflection;
using LanguageReader.Shared.Constants;

namespace LanguageReader.Api.Features.SystemInfo;

internal static class SystemInfoEndpoints
{
    public static IEndpointRouteBuilder MapSystemInfoEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new
        {
            status = "Healthy",
            application = ApplicationConstants.ApplicationName,
            timestampUtc = DateTimeOffset.UtcNow
        }))
        .WithName("Health")
        .WithOpenApi();

        app.MapGet("/version", () => Results.Ok(new
        {
            application = ApplicationConstants.ApplicationName,
            version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0"
        }))
        .WithName("Version")
        .WithOpenApi();

        return app;
    }
}

