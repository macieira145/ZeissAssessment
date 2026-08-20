namespace ZeissAssessment.Application.Exceptions;

public class ValidationException : AppException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors) : base(
        "One or more validation errors have occurred.", Exceptions.ErrorCode.ValidationFailed)
    {
        Errors = errors;
    }
}