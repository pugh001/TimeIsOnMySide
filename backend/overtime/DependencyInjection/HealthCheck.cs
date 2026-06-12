using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Overtime.DependencyInjection;

public static class HealthCheck
{
    public static WebApplication ConfigureHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health/liveness", new HealthCheckOptions { Predicate = _ => false });
        app.MapHealthChecks("/health/readiness");
        return app;
    }
}
