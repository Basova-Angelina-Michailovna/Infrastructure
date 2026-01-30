using System.Net;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace StretchRoom.Infrastructure.Extensions;

/// <summary>
///     The <see cref="SrServerExtensions" /> class.
/// </summary>
[PublicAPI]
public static class SrServerExtensions
{
    private static readonly Dictionary<int, string> StatusCodes = Enum.GetValues<HttpStatusCode>()
        .DistinctBy(x => (int)x).ToDictionary(x => (int)x, x => x.ToString());

    extension(ProblemDetails)
    {
        /// <summary>
        ///     Creates the new instance of <see cref="ProblemDetails" /> by <paramref name="ctx" />.
        /// </summary>
        /// <param name="ctx">The http response.</param>
        /// <returns>The new instance of <see cref="ProblemDetails" />.</returns>
        public static ProblemDetails CreateFromResponse(HttpResponse ctx)
        {
            return new ProblemDetails
            {
                Status = ctx.StatusCode,
                Instance = ctx.HttpContext.Request.Path,
                Title = StatusCodes.TryGetValue(ctx.HttpContext.Response.StatusCode, out var statusCode)
                    ? statusCode
                    : ctx.HttpContext.Response.StatusCode.ToString()
            };
        }
    }
}