namespace RestService.Helpers.IdentityProvider;

/// <summary>
/// Signature used bly IDP Client
/// </summary>
public interface IIdentityProviderClient
{
    Task<BearerToken> GetToken(string username, string password);
}