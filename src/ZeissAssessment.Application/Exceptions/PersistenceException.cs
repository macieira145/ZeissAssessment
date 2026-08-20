namespace ZeissAssessment.Application.Exceptions;

public class PersistenceException : AppException
{
    public PersistenceException(string message)
        : base(message, ErrorCode.InternalError)
    {
    }

    public static PersistenceException DeleteFailed(string entityName, object key)
        => new($"Failed to delete {entityName} with id {key}.");

    public static PersistenceException SaveFailed(string entityName, object key)
        => new($"Failed to save {entityName} with id {key}.");
}