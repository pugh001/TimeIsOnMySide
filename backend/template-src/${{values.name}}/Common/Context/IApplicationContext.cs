namespace RestService.Common.Context;

/// <summary>
/// Interface used by the Application Context. Properties listed below are the state values being stored for the application.
/// </summary>
public interface IApplicationContext
{
    string CorrelationId { get; }
    public string CorrelationKey { get; }
    string SetCorrelationId();
}