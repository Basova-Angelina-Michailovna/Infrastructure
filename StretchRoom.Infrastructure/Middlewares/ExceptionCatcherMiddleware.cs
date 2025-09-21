using Microsoft.AspNetCore.Http;
using StretchRoom.Infrastructure.Exceptions;

namespace StretchRoom.Infrastructure.Middlewares;

internal sealed class ExceptionCatcherMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext httpContext)
    {
        try
        {
            await next.Invoke(httpContext);
        }
        catch (Exception ex)
        {
            ApiExceptionHelper.ThrowException(ex);
        }
    }
}