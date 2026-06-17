using RestService.DependencyInjection.Models;

namespace RestService.Helpers.IdentityProvider;

/// <summary>
/// Interface representing the configuration required by the Identity provider client.
/// </summary>
public interface IIdentityProviderConfig
{
    string ProviderURL { get; set; }
    string ClientID { get; set; }
    string ClientIDSecretKey { get; set; }
    ServiceAccount ServiceAccount { get; set; }
}