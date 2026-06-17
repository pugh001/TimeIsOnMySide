namespace RestService.Common.Exceptions;

/// <summary>
/// Custom exception for database errors as an example
/// </summary>
public class CustomDatabaseException : Exception
{
    public CustomDatabaseException()
    {
    }

    public CustomDatabaseException(string message) : base(message)
    {
    }

    public CustomDatabaseException(string message, Exception innerException) :
        base(message, innerException)
    {
    }
}