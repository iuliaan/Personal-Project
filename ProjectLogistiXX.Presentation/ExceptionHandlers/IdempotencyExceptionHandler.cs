using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProjectLogistiXX.Application.Abstraction.Exceptions.Idempotency;

namespace ProjectLogistiXX.Presentation.ExceptionHandlers;

public sealed class IdempotencyExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, retryAfter) = exception switch
        {
            IdempotencyTransientException => (StatusCodes.Status409Conflict, "Request already in progress", true),
            IdempotencyFatalException => (StatusCodes.Status500InternalServerError, "Idempotency processing failed", false),
            _ => (0, string.Empty, false)
        };

        if (statusCode == 0)
        {
            return false;
        }

        if (retryAfter)
        {
            httpContext.Response.Headers.RetryAfter = "1";
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message
        }, cancellationToken);

        return true;
    }
}
