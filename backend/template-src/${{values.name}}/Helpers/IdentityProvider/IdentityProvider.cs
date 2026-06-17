using System.Runtime.Serialization;

namespace RestService.Helpers.IdentityProvider;

/// <summary>
/// Exception class used by the Identity Provider code to raise exceptions specific to the Identity provider.
/// </summary>
[Serializable]
public class IdentityProvider : Exception
{
    public IdentityProvider()
    {
    }

    public IdentityProvider(string message) : base(message)
    {
    }

    public IdentityProvider(string message, Exception innerException) :
        base(message, innerException)
    {
    }

    protected IdentityProvider(SerializationInfo info, StreamingContext context) :
        base(info, context)
    {
    }
}