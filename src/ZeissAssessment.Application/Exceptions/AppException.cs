namespace ZeissAssessment.Application.Exceptions;

public abstract class AppException : Exception
{
    public ErrorCode ErrorCode { get; }

    protected AppException(string message, ErrorCode errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }
}