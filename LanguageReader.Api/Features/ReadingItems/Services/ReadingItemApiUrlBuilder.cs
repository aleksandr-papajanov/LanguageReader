using LanguageReader.Infrastructure.Features.ReadingItems.Entities;

namespace LanguageReader.Api.Features.ReadingItems;

internal sealed class ReadingItemApiUrlBuilder(
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration)
{
    public string? GetCoverImageUrl(ReadingItemEntity item, string? username)
    {
        var relativeUrl = ReadingItemFeatureHelpers.GetCoverImageUrl(item, username);
        return GetImageUrl(relativeUrl);
    }

    public string? GetImageUrl(string? relativeUrl)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl))
        {
            return relativeUrl;
        }

        if (Uri.TryCreate(relativeUrl, UriKind.Absolute, out _))
        {
            return GetPublicUrl($"/api/reading-items/image-proxy?url={Uri.EscapeDataString(relativeUrl)}");
        }

        return GetPublicUrl(relativeUrl);
    }

    private string GetPublicUrl(string relativeUrl)
    {
        var configuredBaseUrl = configuration["Api:PublicBaseUrl"];
        if (!string.IsNullOrWhiteSpace(configuredBaseUrl)
            && Uri.TryCreate(configuredBaseUrl, UriKind.Absolute, out var configuredBaseUri)
            && Uri.TryCreate(configuredBaseUri, relativeUrl.TrimStart('/'), out var configuredUri))
        {
            return configuredUri.ToString();
        }

        var request = httpContextAccessor.HttpContext?.Request;
        if (request is null)
        {
            return relativeUrl;
        }

        return $"{request.Scheme}://{request.Host}{relativeUrl}";
    }
}
