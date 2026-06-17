namespace RestService.Common.Exceptions;

/// <summary>
/// Exception class used by the application when expected configuration is missing from the config.
/// </summary>
public class ConfigurationException : Exception
{
    public ConfigurationException()
    {
    }

    public ConfigurationException(string message) : base(message)
    {
    }

    public ConfigurationException(string message, Exception innerException) :
        base(message, innerException)
    {
    }

    public ConfigurationException(string name, object property) :
        base($"Configuration \"{name}\" ({property}) was not set.")
    {
    }
}