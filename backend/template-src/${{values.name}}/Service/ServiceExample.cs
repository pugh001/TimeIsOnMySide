using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using RestService.Common.Context;
using RestService.Common.Exceptions;
using RestService.DependencyInjection.Models;
using RestService.Helpers.IdentityProvider;
using RestService.Service.Models.CreateExample;
using RestService.Service.Models.DeleteExample;
using RestService.Service.Models.RetrieveFromDatabaseExample;
using RestService.Service.Models.RetrieveFromServiceExample;

namespace RestService.Service;

/// <summary>
/// Example of a manager class containing the logic of the service.
/// Put application related logic here.
/// </summary>
public class ServiceExample : IServiceExample
{
    // Config is injected to read value from Vault.
    private readonly IConfiguration _configuration;

    // All below is IDP related.
    private readonly TokenCache<ServiceExample> _tokenCache;
    private readonly IOptions<IdentityProviderConfig> _identityProviderConfig;
    private readonly IIdentityProviderClient _identityProviderClient;
    private readonly string _serviceAccountPassword;

    // Http related to make calls to services
    private readonly IOptions<DomainUriOptions> _domainUriConfig;
    private readonly IApplicationContext _applicationContext;

    /// <summary>
    /// Constructor with configuration models injected.
    /// </summary>
    public ServiceExample(IOptions<DomainUriOptions> domainUriConfig,
        IOptions<IdentityProviderConfig> identityProviderConfig,
        IIdentityProviderClient identityProviderClient, 
        IConfiguration configuration, IApplicationContext applicationContext)
    {
        // Config is injected to read value from Vault.
        _configuration = configuration ??
                         throw new ConfigurationException($"{nameof(_configuration)} is missing");

        // Validate all config is present to make a outgoing http call.
        _domainUriConfig = _domainUriConfig ??
                           throw new
                               ConfigurationException($"{nameof(_domainUriConfig)} is missing");
       
        _applicationContext = applicationContext ??
                              throw new
                                  ConfigurationException($"{nameof(applicationContext)} is missing");

        // Validate that required configuration is present for the IDP.
        _tokenCache = TokenCache<ServiceExample>.GetInstance() ??
                      throw new ConfigurationException($"{nameof(_tokenCache)} is missing");
        _identityProviderConfig = identityProviderConfig ??
                                  throw new
                                      ConfigurationException($"{nameof(_identityProviderConfig)} is missing");
        _identityProviderClient = identityProviderClient ??
                                  throw new
                                      ConfigurationException($"{nameof(_identityProviderClient)} is missing");
        _serviceAccountPassword =
            _configuration.GetValue<string>(_identityProviderConfig.Value.ServiceAccount
                                                .PasswordKey) ??
            throw new
                ConfigurationException($"Secret for key '{_identityProviderConfig.Value.ServiceAccount.PasswordKey}' is missing");

        // Authenticate with the IDP and stored the token. The manager is scoped transient so we can request the token in the constructor.
        BearerToken bearerToken = _tokenCache.GetToken(_identityProviderClient,
                                                       _identityProviderConfig.Value.ServiceAccount
                                                           .Username, _serviceAccountPassword)
                                             .Result;
        if (bearerToken == null) throw new ValidationException($"Token '{bearerToken}' is null");
        if (string.IsNullOrEmpty(bearerToken.AccessToken))
            throw new ArgumentException($"Token '{bearerToken.AccessToken}' is missing");
    }

    public Task<bool> Delete(DeleteRequest request)
    {
        request.Validate();

        throw new NotImplementedException();
    }

    public Task<RetrieveFromDatabaseResponse> RetrieveFromDatabase(RetrieveFromDatabaseRequest request)
    {
        request.Validate();

        throw new NotImplementedException();
    }

    public Task<RetrieveFromServiceResponse> RetrieveFromService(RetrieveFromServiceRequest request)
    {
        request.Validate();

        throw new NotImplementedException();
    }
    
    public Task Create(CreateRequest request)
    {
        request.Validate();

        throw new NotImplementedException();
    }
}