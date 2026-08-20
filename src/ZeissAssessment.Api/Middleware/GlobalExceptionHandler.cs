using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ZeissAssessment.Application.Exceptions;
using ZeissAssessment.Domain.Exceptions;

namespace ZeissAssessment.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = BuildProblemDetails(exception);
        Log(exception, problemDetails);

        httpContext.Response.StatusCode = problemDetails.Status!.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static ProblemDetails BuildProblemDetails(Exception exception)
    {
        var problemDetails = exception switch
        {
            ValidationException validationException => new ProblemDetails
            {
                Status = validationException.ErrorCode.StatusCode,
                Title = validationException.Message,
                Extensions = { ["errors"] = validationException.Errors }
            },

            AppException appException => new ProblemDetails
            {
                Status = appException.ErrorCode.StatusCode,
                Title = appException.Message
            },

            DomainException domainException => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = domainException.Message
            },

            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred."
            }
        };

        problemDetails.Extensions["errorCode"] = GetErrorCode(exception);
        problemDetails.Type = $"https://httpstatuses.io/{problemDetails.Status}";

        return problemDetails;
    }

    private static string GetErrorCode(Exception exception) => exception switch
    {
        AppException appException => appException.ErrorCode.Code,
        DomainException => "DOMAIN_RULE_VIOLATION",
        _ => "INTERNAL_ERROR"
    };

    private void Log(Exception exception, ProblemDetails problemDetails)
    {
        if (problemDetails.Status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception");
        else
            logger.LogWarning(exception, "Handled exception: {ErrorCode}", problemDetails.Extensions["errorCode"]);
    }
}