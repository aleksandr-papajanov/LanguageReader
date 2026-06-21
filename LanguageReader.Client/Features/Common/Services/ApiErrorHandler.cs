using System.Net.Http.Json;

namespace LanguageReader.Client.Features.Common.Services;

public static class ApiErrorHandler
{
    public static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var error = await TryReadApiErrorAsync(response, cancellationToken);
        var message = BuildErrorMessage(response, error);

        LogApiError(response, error, message);

        throw new InvalidOperationException(message);
    }

    private static async Task<ApiErrorResponse?> TryReadApiErrorAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<ApiErrorResponse>(cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private static string BuildErrorMessage(
        HttpResponseMessage response,
        ApiErrorResponse? error)
    {
        if (error is null)
        {
            return $"Request failed: {(int)response.StatusCode} {response.ReasonPhrase}";
        }

        if (error.Errors is null || error.Errors.Count == 0)
        {
            return error.Message;
        }

        var details = string.Join(
            "; ",
            error.Errors.SelectMany(pair =>
                pair.Value.Select(value => $"{pair.Key}: {value}")));

        return $"{error.Message} {details}".Trim();
    }

    private static void LogApiError(
        HttpResponseMessage response,
        ApiErrorResponse? error,
        string message)
    {
        Console.Error.WriteLine(
            $"[LanguageReader API] {(int)response.StatusCode} {response.RequestMessage?.Method} {response.RequestMessage?.RequestUri}");

        Console.Error.WriteLine($"[LanguageReader API] {message}");

        if (!string.IsNullOrWhiteSpace(error?.Detail))
        {
            Console.Error.WriteLine(error.Detail);
        }
    }
}
