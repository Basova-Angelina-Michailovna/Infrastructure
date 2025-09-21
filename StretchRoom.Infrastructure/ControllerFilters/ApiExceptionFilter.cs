using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using StretchRoom.Infrastructure.Exceptions;
using StretchRoom.Infrastructure.Models;

namespace StretchRoom.Infrastructure.ControllerFilters;

/// <summary>
///     The <see cref="ApiExceptionFilter" /> class.
/// </summary>
/// <param name="logger">The logger.</param>
public class ApiExceptionFilter(ILogger<ApiExceptionFilter> logger) : ExceptionFilterAttribute
{
    /// <inheritdoc />
    public override void OnException(ExceptionContext context)
    {
        logger.LogError(context.Exception, "Error on request processing!");
        if (context.Exception is ApiException apiException)
        {
            context.Result = new ProblemDetailsResult(apiException.ProblemDetails);
            return;
        }

        ProblemDetails details = new()
        {
            Status = StatusCodes.Status500InternalServerError,
            Detail = context.Exception.Message,
            Instance = FormatDetail(context.HttpContext.Request),
            Title = "Internal error!"
        };
        context.Result = new ProblemDetailsResult(details);
    }

    private static string FormatDetail(HttpRequest request)
    {
        return $"{request.Method}: {request.Path}";
    }
}