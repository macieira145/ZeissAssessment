namespace ZeissAssessment.Application.Exceptions;

public class ConflictException : AppException
{
    public ConflictException(string message) : base(message, ErrorCode.Conflict)
    {
    }
}