using System.Globalization;
using System.Net.Http.Json;
using System.Reflection;

namespace LanguageReader.Client.Features.Common.Services;

public sealed class ApiClient(HttpClient httpClient)
{
    public Task<TResponse> GetAsync<TResponse>(
        string routeTemplate,
        object? request = null,
        CancellationToken ct = default)
    {
        var builtRoute = ApiRouteBuilder.BuildQuery(routeTemplate, request);

        return SendAsync<TResponse>(
            HttpMethod.Get,
            builtRoute.Url,
            body: null,
            ct);
    }

    public Task<TResponse> PostAsync<TResponse>(
        string routeTemplate,
        object body,
        CancellationToken ct)
    {
        return PostAsync<TResponse>(
            routeTemplate,
            routeValues: null,
            body: body,
            ct: ct);
    }

    public Task<TResponse> PostAsync<TResponse>(
        string routeTemplate,
        object? routeValues = null,
        object? body = null,
        CancellationToken ct = default)
    {
        var builtRoute = ApiRouteBuilder.BuildBody(routeTemplate, routeValues, body);

        return SendAsync<TResponse>(
            HttpMethod.Post,
            builtRoute.Url,
            builtRoute.Body,
            ct);
    }

    public Task<TResponse> PutAsync<TResponse>(
        string routeTemplate,
        object body,
        CancellationToken ct)
    {
        return PutAsync<TResponse>(
            routeTemplate,
            routeValues: null,
            body: body,
            ct: ct);
    }

    public Task<TResponse> PutAsync<TResponse>(
        string routeTemplate,
        object? routeValues = null,
        object? body = null,
        CancellationToken ct = default)
    {
        var builtRoute = ApiRouteBuilder.BuildBody(routeTemplate, routeValues, body);

        return SendAsync<TResponse>(
            HttpMethod.Put,
            builtRoute.Url,
            builtRoute.Body,
            ct);
    }

    public async Task PutAsync(
        string routeTemplate,
        object? routeValues = null,
        object? body = null,
        CancellationToken ct = default)
    {
        var builtRoute = ApiRouteBuilder.BuildBody(routeTemplate, routeValues, body);

        using var message = new HttpRequestMessage(HttpMethod.Put, builtRoute.Url);

        if (builtRoute.Body is not null)
        {
            message.Content = JsonContent.Create(builtRoute.Body);
        }

        using var response = await httpClient.SendAsync(message, ct);
        await ApiErrorHandler.EnsureSuccessAsync(response, ct);
    }

    public async Task DeleteAsync(
        string routeTemplate,
        object? request = null,
        CancellationToken ct = default)
    {
        var builtRoute = ApiRouteBuilder.BuildQuery(routeTemplate, request);

        using var response = await httpClient.DeleteAsync(builtRoute.Url, ct);
        await ApiErrorHandler.EnsureSuccessAsync(response, ct);
    }

    public async Task<TResponse> SendMultipartAsync<TResponse>(
        string url,
        MultipartFormDataContent form,
        CancellationToken ct = default)
    {
        using var response = await httpClient.PostAsync(url, form, ct);

        await ApiErrorHandler.EnsureSuccessAsync(response, ct);

        return await response.Content.ReadFromJsonAsync<TResponse>(ct)
            ?? throw new InvalidOperationException("The API returned an empty response.");
    }

    private async Task<TResponse> SendAsync<TResponse>(
        HttpMethod method,
        string url,
        object? body,
        CancellationToken ct)
    {
        using var message = new HttpRequestMessage(method, url);

        if (body is not null)
        {
            message.Content = JsonContent.Create(body);
        }

        using var response = await httpClient.SendAsync(message, ct);
        await ApiErrorHandler.EnsureSuccessAsync(response, ct);

        return await response.Content.ReadFromJsonAsync<TResponse>(ct)
            ?? throw new InvalidOperationException("The API returned an empty response.");
    }
}

internal sealed record BuiltApiRoute(string Url, object? Body);

internal static class ApiRouteBuilder
{
    public static BuiltApiRoute BuildQuery(
        string routeTemplate,
        object? request)
    {
        if (request is null)
        {
            return new BuiltApiRoute(routeTemplate, null);
        }

        var values = RequestObjectReader.ToDictionary(request);
        var url = ReplaceRouteParameters(routeTemplate, values);
        url = AppendQuery(url, values);

        return new BuiltApiRoute(url, null);
    }

    public static BuiltApiRoute BuildBody(
        string routeTemplate,
        object? routeValues,
        object? body)
    {
        var values = routeValues is null
            ? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            : RequestObjectReader.ToDictionary(routeValues);

        var url = ReplaceRouteParameters(routeTemplate, values);

        // Если body отдельно не передали, то остаток routeValues идёт в body.
        var finalBody = body ?? (values.Count > 0 ? values : null);

        return new BuiltApiRoute(url, finalBody);
    }

    private static string ReplaceRouteParameters(
        string routeTemplate,
        Dictionary<string, object?> values)
    {
        var result = routeTemplate;

        foreach (var pair in values.ToArray())
        {
            var token = "{" + pair.Key + "}";

            if (!result.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (pair.Value is null)
            {
                throw new InvalidOperationException(
                    $"Route parameter '{pair.Key}' cannot be null.");
            }

            result = result.Replace(
                token,
                Uri.EscapeDataString(FormatValue(pair.Value)),
                StringComparison.OrdinalIgnoreCase);

            values.Remove(pair.Key);
        }

        return result;
    }

    private static string AppendQuery(
        string url,
        Dictionary<string, object?> values)
    {
        var queryItems = values
            .Where(x => x.Value is not null)
            .Select(x =>
                $"{Uri.EscapeDataString(ToCamelCase(x.Key))}={Uri.EscapeDataString(FormatValue(x.Value!))}")
            .ToArray();

        if (queryItems.Length == 0)
        {
            return url;
        }

        var separator = url.Contains('?') ? "&" : "?";

        return url + separator + string.Join("&", queryItems);
    }

    private static string FormatValue(object value)
    {
        return value switch
        {
            bool boolValue => boolValue.ToString().ToLowerInvariant(),
            DateTime dateTime => dateTime.ToString("O"),
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("O"),
            Guid guid => guid.ToString(),
            Enum enumValue => enumValue.ToString(),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
    }

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return char.ToLowerInvariant(value[0]) + value[1..];
    }
}

internal static class RequestObjectReader
{
    public static Dictionary<string, object?> ToDictionary(object request)
    {
        return request
            .GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.GetMethod is not null)
            .ToDictionary(
                property => property.Name,
                property => property.GetValue(request),
                StringComparer.OrdinalIgnoreCase);
    }
}
