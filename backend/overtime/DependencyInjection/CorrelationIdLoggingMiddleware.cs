using Overtime.Common.Context;
using Serilog.Context;

namespace Overtime.DependencyInjection;

public sealed class CorrelationIdLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdLoggingMiddleware(RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(next);
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IApplicationContext appContext)
    {
        using (LogContext.PushProperty("CorrelationId", appContext.CorrelationId))
        {
            await _next(context);
        }
    }
}
