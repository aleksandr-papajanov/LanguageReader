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

        throw new ApiClientException(
            message,
            response.StatusCode,
            response.RequestMessage?.Method?.Method,
            response.RequestMessage?.RequestUri?.ToString(),
            error);
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

        var message = string.IsNullOrWhiteSpace(error.Message)
            ? $"Request failed: {(int)response.StatusCode} {response.ReasonPhrase}"
            : error.Message.Trim();

        if (error.Errors is null || error.Errors.Count == 0)
        {
            return AppendTraceId(message, error.TraceId);
        }

        var details = string.Join(
            "; ",
            error.Errors.SelectMany(pair =>
                pair.Value.Select(value => $"{pair.Key}: {value}")));

        return AppendTraceId($"{message} {details}".Trim(), error.TraceId);
    }

    private static void LogApiError(
        HttpResponseMessage response,
        ApiErrorResponse? error,
        string message)
    {
        Console.Error.WriteLine(
            $"[LanguageReader API] {(int)response.StatusCode} {response.RequestMessage?.Method} {response.RequestMessage?.RequestUri}");

        Console.Error.WriteLine($"[LanguageReader API] {message}");

        if (error is not null)
        {
            Console.Error.WriteLine(
                $"[LanguageReader API] type={error.Type}; status={error.StatusCode}; traceId={error.TraceId ?? "n/a"}");
        }

        if (!string.IsNullOrWhiteSpace(error?.Detail))
        {
            Console.Error.WriteLine(error.Detail);
        }
    }

    private static string AppendTraceId(string message, string? traceId)
    {
        return string.IsNullOrWhiteSpace(traceId)
            ? message
            : $"{message} Trace id: {traceId}";
    }
}

public sealed class ApiClientException(
    string message,
    System.Net.HttpStatusCode statusCode,
    string? method,
    string? requestUri,
    ApiErrorResponse? error)
    : InvalidOperationException(message)
{
    public System.Net.HttpStatusCode StatusCode { get; } = statusCode;

    public string? Method { get; } = method;

    public string? RequestUri { get; } = requestUri;

    public ApiErrorResponse? Error { get; } = error;
}
