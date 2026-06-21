using System.Net;
using LanguageReader.Infrastructure.Exceptions;

namespace LanguageReader.Api.Middleware;

/// <summary>
/// Converts application exceptions into standardized API responses.
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment environment)
{
    /// <summary>
    /// Handles the current request.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException exception)
        {
            await WriteErrorAsync(context, "validation_error", exception.Message, HttpStatusCode.BadRequest, exception.Errors);
        }
        catch (BadHttpRequestException exception)
        {
            await WriteErrorAsync(context, "bad_request", exception.Message, (HttpStatusCode)exception.StatusCode, detail: GetDevelopmentDetail(exception));
        }
        catch (DomainException exception)
        {
            await WriteErrorAsync(context, "domain_error", exception.Message, HttpStatusCode.Conflict);
        }
        catch (ForbiddenException exception)
        {
            await WriteErrorAsync(context, "forbidden", exception.Message, HttpStatusCode.Forbidden);
        }
        catch (NotFoundException exception)
        {
            await WriteErrorAsync(context, "not_found", exception.Message, HttpStatusCode.NotFound);
        }
        catch (InfrastructureException exception)
        {
            logger.LogError(exception, "Infrastructure error");
            await WriteErrorAsync(context, "infrastructure_error", exception.Message, HttpStatusCode.ServiceUnavailable, detail: GetDevelopmentDetail(exception));
        }
        catch (Infrastructure.Exceptions.ApplicationException exception)
        {
            await WriteErrorAsync(context, "application_error", exception.Message, HttpStatusCode.BadRequest);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled error");
            await WriteErrorAsync(context, "unexpected_error", "An unexpected error occurred.", HttpStatusCode.InternalServerError, detail: GetDevelopmentDetail(exception));
        }
    }

    private static async Task WriteErrorAsync(
        HttpContext context,
        string type,
        string message,
        HttpStatusCode statusCode,
        IReadOnlyDictionary<string, string[]>? errors = null,
        string? detail = null)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new ApiErrorResponse(
            type,
            message,
            (int)statusCode,
            errors,
            context.TraceIdentifier,
            detail);

        await context.Response.WriteAsJsonAsync(response);
    }

    private string? GetDevelopmentDetail(Exception exception)
    {
        return environment.IsDevelopment()
            ? exception.ToString()
            : null;
    }
}
