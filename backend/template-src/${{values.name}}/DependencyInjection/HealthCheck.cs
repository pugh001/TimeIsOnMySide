using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace RestService.DependencyInjection;

/// <summary>
///  Configures Health checks
/// </summary>
public static class HealthCheck
{
    public static WebApplication ConfigureHealthChecks(this WebApplication app)
    {
        // The application is ready to serve traffic.
        app.MapHealthChecks("/health/liveness",
                            new HealthCheckOptions() { Predicate = (_) => false });

        // All the dependencies are ready.
        app.MapHealthChecks("/health/readiness", new HealthCheckOptions { });

        return app;
    }
}