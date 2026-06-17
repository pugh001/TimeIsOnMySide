using RestService.Common.Context;
using RestService.Service;

namespace RestService.DependencyInjection;

/// <summary>
/// Added dependencies to be injected for the service / application
/// </summary>
public static class ApplicationDependencies
{
    public static IServiceCollection ConfigureApplicationDependencies(
        this IServiceCollection services)
    {
        services.AddScoped<IApplicationContext, ApplicationContext>();
        services.AddTransient<IServiceExample, ServiceExample>();

        return services;
    }
}