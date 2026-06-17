using System.Net.Http.Headers;
using RestService.Common.Exceptions;
using RestService.DependencyInjection.Models;
using RestService.Helpers.IdentityProvider;

namespace RestService.DependencyInjection;

/// <summary>
/// Read Identity provider configuration and hosts a local http client for the IDP client.
/// </summary>
public static class IdentityProvider
{
    public static IServiceCollection ConfigureIdentityProvider(this IServiceCollection services,
        WebApplicationBuilder builder)
    {
        services.Configure<IdentityProviderConfig>(builder.Configuration
                                                          .GetSection("IdentityProvider") ??
                                                   throw new
                                                       ConfigurationException($"Configuration for key 'IdentityProvider' is missing"));

        var idpConfig = builder.Configuration.GetSection("IdentityProvider")
                               .Get<IdentityProviderConfig>();

        if (String.IsNullOrEmpty(idpConfig.ProviderURL))
            throw new
                ConfigurationException($"Configuration path for 'IdentityProvider:ProviderURL' is empty");

        services.AddHttpClient<IIdentityProviderClient, IdentityProviderClient>(client =>
        {
            client.BaseAddress = new Uri(idpConfig.ProviderURL);
            client.DefaultRequestHeaders.Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        return services;
    }
}