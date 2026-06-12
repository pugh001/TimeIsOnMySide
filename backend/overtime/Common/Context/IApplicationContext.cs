namespace Overtime.Common.Context;

public interface IApplicationContext
{
    string CorrelationId { get; }
}
