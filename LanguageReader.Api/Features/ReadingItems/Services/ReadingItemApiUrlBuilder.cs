using LanguageReader.Infrastructure.Features.ReadingItems.Entities;

namespace LanguageReader.Api.Features.ReadingItems;

internal sealed class ReadingItemApiUrlBuilder(IHttpContextAccessor httpContextAccessor)
{
    public string? GetCoverImageUrl(ReadingItemEntity item, string? username)
    {
        var relativeUrl = ReadingItemFeatureHelpers.GetCoverImageUrl(item, username);
        if (string.IsNullOrWhiteSpace(relativeUrl))
        {
            return relativeUrl;
        }

        if (Uri.TryCreate(relativeUrl, UriKind.Absolute, out _))
        {
            return relativeUrl;
        }

        var request = httpContextAccessor.HttpContext?.Request;
        if (request is null)
        {
            return relativeUrl;
        }

        return $"{request.Scheme}://{request.Host}{relativeUrl}";
    }
}
