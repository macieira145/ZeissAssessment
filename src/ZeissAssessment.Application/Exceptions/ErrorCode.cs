using Microsoft.AspNetCore.Http;

namespace ZeissAssessment.Application.Exceptions;

public sealed record ErrorCode(string Code, int StatusCode)
{
    public static readonly ErrorCode NotFound = new("NOT_FOUND", StatusCodes.Status404NotFound);
    public static readonly ErrorCode ValidationFailed = new("VALIDATION_FAILED", StatusCodes.Status400BadRequest);
    public static readonly ErrorCode Conflict = new("CONFLICT", StatusCodes.Status409Conflict);
    public static readonly ErrorCode InternalError = new("INTERNAL_ERROR", StatusCodes.Status500InternalServerError);
}