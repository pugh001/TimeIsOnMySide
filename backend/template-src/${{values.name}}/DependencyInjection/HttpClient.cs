using RestService.DependencyInjection.Models;

namespace RestService.DependencyInjection;

public static class HttpClient
{
    /// <summary>
    /// Define named Http Clients to allow for reuse of HTTP clients.
    /// If Http Clients are not re-used it results in socket exhaustion.
    /// </summary>
    public static void ConfigureHttpClients(this IServiceCollection services,
        WebApplicationBuilder builder)
    {
        // Replace DomainEndpoints with the value from your config.
        var domainEndpoints =
            builder.Configuration.GetSection("DomainEndpoints").Get<DomainUriOptions>();

        services.AddHttpClient("Domain",
                               client =>
                               {
                                   client.BaseAddress = new Uri(domainEndpoints.GatewayURL);
                               });
    }
}