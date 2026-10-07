using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SharedKernel.Application;

namespace SharedKernel.Infrastructure;

public sealed class ApiErrorMiddleware
{
    public const string CorrelationHeader = "X-Correlation-Id";

    private readonly RequestDelegate _next;
    private readonly ILogger<ApiErrorMiddleware> _logger;

    public ApiErrorMiddleware(RequestDelegate next, ILogger<ApiErrorMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context);
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationHeader] = correlationId;
            return Task.CompletedTask;
        });

        try
        {
            await _next(context);
        }
        catch (AppException exception)
        {
            _logger.LogInformation(
                "API error {Code} {StatusCode}. CorrelationId: {CorrelationId}",
                exception.Code,
                exception.StatusCode,
                correlationId);

            await WriteErrorAsync(context, exception.StatusCode, exception.Code, correlationId);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unhandled exception. CorrelationId: {CorrelationId}",
                correlationId);

            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, "internal_error", correlationId);
        }
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationHeader, out var header))
        {
            var value = header.ToString().Trim();
            if (value.Length > 0)
            {
                return value;
            }
        }

        return context.TraceIdentifier;
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string code, string correlationId)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(
            new ApiError(code, correlationId),
            context.RequestAborted);
    }
}
