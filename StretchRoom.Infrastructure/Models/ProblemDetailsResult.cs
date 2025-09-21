using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace StretchRoom.Infrastructure.Models;

/// <summary>
///     The <see cref="ProblemDetailsResult" /> class.
/// </summary>
[PublicAPI]
public class ProblemDetailsResult : ObjectResult
{
    /// <summary>
    ///     Initiates the new instance of <see cref="ProblemDetailsResult" />.
    /// </summary>
    /// <param name="value"></param>
    public ProblemDetailsResult(ProblemDetails value) : base(value)
    {
        StatusCode = value.Status;
        ContentTypes = ["application/problem+json"];
    }
}