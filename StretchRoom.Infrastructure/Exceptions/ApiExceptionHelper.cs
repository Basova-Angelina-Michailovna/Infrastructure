using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace StretchRoom.Infrastructure.Exceptions;

/// <summary>
///     The <see cref="ApiException" /> helper class.
/// </summary>
[PublicAPI]
public static class ApiExceptionHelper
{
    /// <summary>
    ///     Throws not found exception.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="memberName">The member name.</param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    /// <exception cref="ApiException"></exception>
    public static TResult ThrowNotFound<TResult>(string message, [CallerMemberName] string memberName = "")
    {
        throw new ApiException(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "NotFound",
            Instance = memberName,
            Detail = message
        }, message);
    }

    /// <summary>
    ///     Throws not found exception.
    /// </summary>
    /// <param name="exception">The inner exception.</param>
    /// <param name="memberName">The member name.</param>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TException"></typeparam>
    /// <returns></returns>
    /// <exception cref="ApiException"></exception>
    public static TResult ThrowNotFound<TResult, TException>(TException exception,
        [CallerMemberName] string memberName = "")
        where TException : Exception
    {
        throw new ApiException(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "NotFound",
            Instance = memberName,
            Detail = exception.Message
        }, exception);
    }

    /// <summary>
    ///     Throws the conflict exception.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="memberName">The member name.</param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    /// <exception cref="ApiException"></exception>
    public static TResult ThrowConflict<TResult>(string message, [CallerMemberName] string memberName = "")
    {
        throw new ApiException(new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Conflict",
            Instance = memberName,
            Detail = message
        }, message);
    }

    /// <summary>
    ///     Throws conflict exception.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="memberName">The member name.</param>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TException"></typeparam>
    /// <returns></returns>
    /// <exception cref="ApiException"></exception>
    public static TResult ThrowConflict<TResult, TException>(TException exception,
        [CallerMemberName] string memberName = "")
        where TException : Exception
    {
        throw new ApiException(new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Conflict",
            Instance = memberName,
            Detail = exception.Message
        }, exception);
    }

    /// <summary>
    ///     Throws the validation exception.
    /// </summary>
    /// <param name="problemDetails">The problem details.</param>
    /// <param name="memberName">The member name.</param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    /// <exception cref="ApiException"></exception>
    public static TResult ThrowValidation<TResult>(ValidationProblemDetails problemDetails,
        [CallerMemberName] string memberName = "")
    {
        throw new ApiException(problemDetails, problemDetails.Title ?? "Validation");
    }

    /// <summary>
    ///     Throws the exception.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="statusCode">The status code.</param>
    /// <param name="memberName">The member name.</param>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TException"></typeparam>
    /// <returns></returns>
    /// <exception cref="ApiException"></exception>
    public static TResult ThrowException<TResult, TException>(TException exception,
        int statusCode = StatusCodes.Status500InternalServerError, [CallerMemberName] string memberName = "")
        where TException : Exception
    {
        throw new ApiException(statusCode, exception, memberName);
    }

    /// <summary>
    ///     Throws the exception.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="statusCode">The status code.</param>
    /// <param name="memberName">The member name.</param>
    /// <typeparam name="TException"></typeparam>
    /// <returns></returns>
    /// <exception cref="ApiException"></exception>
    public static void ThrowException<TException>(TException exception,
        int statusCode = StatusCodes.Status500InternalServerError, [CallerMemberName] string memberName = "")
        where TException : Exception
    {
        throw new ApiException(statusCode, exception, memberName);
    }

    /// <summary>
    ///     Throws the api exception.
    /// </summary>
    /// <param name="problemDetails">The problem details.</param>
    /// <param name="statusCode">The status code.</param>
    /// <param name="memberName">The member name.</param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    /// <exception cref="ApiException"></exception>
    public static TResult ThrowApiException<TResult>(ProblemDetails? problemDetails, int? statusCode = null,
        [CallerMemberName] string memberName = "")
    {
        ThrowApiException(problemDetails, statusCode, memberName);
        return default!;
    }

    /// <summary>
    ///     Throws the api exception.
    /// </summary>
    /// <param name="problemDetails">The problem details.</param>
    /// <param name="statusCode">The status code.</param>
    /// <param name="memberName">The member name.</param>
    /// <returns></returns>
    /// <exception cref="ApiException"></exception>
    public static void ThrowApiException(ProblemDetails? problemDetails, int? statusCode = null,
        [CallerMemberName] string memberName = "")
    {
        statusCode ??= problemDetails?.Status ?? -1;
        if (problemDetails is null)
        {
            problemDetails ??= new ProblemDetails
            {
                Status = statusCode,
                Title = "API Error",
                Detail = "Null problem details",
                Instance = memberName
            };
            throw new ApiException(problemDetails, "Null problem details");
        }

        throw new ApiException(problemDetails, problemDetails.Title ?? "Problem details");
    }
}