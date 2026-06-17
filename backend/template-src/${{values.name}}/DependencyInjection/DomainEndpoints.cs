using RestService.Common.Exceptions;
using RestService.DependencyInjection.Models;
using Serilog;

namespace RestService.DependencyInjection;

/// <summary>
/// Reads endpoint url configuration into a model.
///
/// The idea is that teams duplicate, rename and adjust this file for every domain they need to work with.
/// </summary>
public static class DomainEndpoints
{
    public static IServiceCollection ReadExampleDomainEndpointConfiguration(
        this IServiceCollection services, WebApplicationBuilder builder)
    {
        Log.Information("Reading 'Example Domain' config");

        services.Configure<DomainUriOptions>(builder.Configuration.GetSection("DomainEndpoints") ??
                                             throw new
                                                 ConfigurationException($"Configuration for key 'DomainEndpoints' is missing"));

        return services;
    }
}