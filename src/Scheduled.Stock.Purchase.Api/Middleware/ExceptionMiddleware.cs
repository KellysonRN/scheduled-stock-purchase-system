using System.Net.Mime;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Scheduled.Stock.Purchase.Api.Middleware;

/// <summary>
/// Middleware responsible for globally handling unhandled exceptions in the application.
/// Implements the <see cref="IExceptionHandler"/> interface to intercept errors and return an appropriate <see cref="ProblemDetails"/>.
/// </summary>
public sealed class ExceptionMiddleware : IExceptionHandler
{
    /// <summary>
    /// Intercepts the thrown exception, configures the <see cref="ProblemDetails"/> according to the error type,
    /// and writes the JSON response to the <see cref="HttpContext.Response"/>.
    /// </summary>
    /// <param name="httpContext">The HTTP request context.</param>
    /// <param name="exception">The captured exception to be handled.</param>
    /// <param name="cancellationToken">Token used to cancel the operation, if necessary.</param>
    /// <returns>
    /// <c>true</c> if the exception was handled and the response was written; otherwise, <c>false</c>.
    /// </returns>
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails problem = new();

        switch (exception)
        {
            case NotImplementedException:
                problem.Type = "https://datatracker.ietf.org/doc/html/rfc9110#name-501-not-implemented";
                problem.Status = StatusCodes.Status501NotImplemented;
                problem.Title = "This feature is not yet available.";
                break;

            default:
                problem.Type = "https://datatracker.ietf.org/doc/html/rfc9110#name-500-internal-server-error";
                problem.Status = StatusCodes.Status500InternalServerError;
                problem.Title = "Internal Server Error. Please try again later.";
                break;
        }

        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = MediaTypeNames.Application.ProblemJson;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}