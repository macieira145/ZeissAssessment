namespace ZeissAssessment.Application.Exceptions;

public class ConcurrencyConflictException : ConflictException
{
    public ConcurrencyConflictException(string entityName, object key) : base(
        $"{entityName} with id '{key}' was modified concurrently. Please retry.")
    {
    }
}
