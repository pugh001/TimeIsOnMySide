using RestService.Common.Context;
using Serilog.Context;

namespace RestService.DependencyInjection;

/// <summary>
/// Middleware that reads the correlation id if present and adds it the log entries.
/// </summary>
public class CorrelationIdLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, IApplicationContext applicationContext)
    {
        var correlationId = applicationContext.CorrelationId;
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next.Invoke(context);
        }
    }
}