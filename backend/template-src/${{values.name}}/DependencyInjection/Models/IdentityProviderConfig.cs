using RestService.Helpers.IdentityProvider;

namespace RestService.DependencyInjection.Models;

public class IdentityProviderConfig : IIdentityProviderConfig
{
    public string ProviderURL { get; set; }

    public string ClientID { get; set; }

    public string ClientIDSecretKey { get; set; }

    public ServiceAccount ServiceAccount { get; set; }

    public IdentityProviderConfig()
    {
    }
}