using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskLists.Api.Logging;
using TaskLists.Application.Exceptions;

namespace TaskLists.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        LogException(exception);

        var problemDetails = exception switch
        {
            ValidationException validationException =>
                CreateValidationProblemDetails(
                    httpContext,
                    validationException),

            NotFoundException notFoundException =>
                CreateProblemDetails(
                    httpContext,
                    StatusCodes.Status404NotFound,
                    notFoundException.Code,
                    notFoundException.Message),

            ForbiddenException forbiddenException =>
                CreateProblemDetails(
                    httpContext,
                    StatusCodes.Status403Forbidden,
                    forbiddenException.Code,
                    forbiddenException.Message),

            ConflictException conflictException =>
                CreateProblemDetails(
                    httpContext,
                    StatusCodes.Status409Conflict,
                    conflictException.Code,
                    conflictException.Message),

            _ =>
                CreateProblemDetails(
                    httpContext,
                    StatusCodes.Status500InternalServerError,
                    "InternalServerError",
                    environment.IsDevelopment()
                        ? exception.ToString()
                        : "An unexpected error occurred.")
        };

        httpContext.Response.StatusCode =
            problemDetails.Status
            ?? StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(
            (object)problemDetails,
            cancellationToken);

        return true;
    }

    private void LogException(Exception exception)
    {
        switch (exception)
        {
            case ValidationException validationException:
                ApplicationLog.ValidationFailed(
                    logger,
                    validationException.Code);
                break;

            case NotFoundException notFoundException:
                ApplicationLog.NotFound(
                    logger,
                    notFoundException.Code);
                break;

            case ForbiddenException forbiddenException:
                ApplicationLog.Forbidden(
                    logger,
                    forbiddenException.Code);
                break;

            case ConflictException conflictException:
                ApplicationLog.Conflict(
                    logger,
                    conflictException.Code);
                break;

            default:
                ApplicationLog.UnhandledException(
                    logger,
                    exception);
                break;
        }
    }

    private static ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        int statusCode,
        string code,
        string detail)
    {
        return new ProblemDetails
        {
            Status = statusCode,
            Title = code,
            Detail = detail,
            Instance = httpContext.Request.Path,
            Extensions =
            {
                ["Code"] = code,
                ["TraceId"] = httpContext.TraceIdentifier
            }
        };
    }

    private static ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext httpContext,
        ValidationException exception)
    {
        return new ValidationProblemDetails(
            exception.Errors.ToDictionary(
                x => x.Key,
                x => x.Value))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = exception.Code,
            Detail = exception.Message,
            Instance = httpContext.Request.Path,
            Extensions =
            {
                ["Code"] = exception.Code,
                ["TraceId"] = httpContext.TraceIdentifier
            }
        };
    }
}