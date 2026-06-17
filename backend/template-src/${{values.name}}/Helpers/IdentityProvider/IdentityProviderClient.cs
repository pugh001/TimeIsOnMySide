using System.Text.Json;
using Microsoft.Extensions.Options;
using RestService.Common.Exceptions;
using RestService.DependencyInjection.Models;
using Serilog;

namespace RestService.Helpers.IdentityProvider;

/// <summary>
/// Client used to integrate with the Identity Provider.
/// </summary>
public class IdentityProviderClient : IIdentityProviderClient
{
    private readonly HttpClient _client;
    private readonly IOptions<IIdentityProviderConfig> _identityProviderConfig;
    private readonly IConfiguration _configuration;
    private readonly string _clientSecretValue;

    public IdentityProviderClient(HttpClient client,
        IOptions<IdentityProviderConfig> identityProviderConfig, IConfiguration configuration)
    {
        _client = client;
        _identityProviderConfig = identityProviderConfig;
        _configuration = configuration;

        _clientSecretValue = _configuration[_identityProviderConfig.Value.ClientIDSecretKey] ??
                             throw new
                                 ConfigurationException($"Secret for key '{_identityProviderConfig.Value.ClientIDSecretKey}' is missing");
    }

    public async Task<BearerToken> GetToken(string username, string password)
    {
        var requestBody = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "username", username },
            { "password", password },
            {
                "client_id",
                _identityProviderConfig.Value
                                       .ClientID
            },
            { "client_secret", _clientSecretValue }
        });

        var response = await _client.PostAsync(_client.BaseAddress, requestBody);

        await HandleHttpResponseAsync(response);

        var content = await response.Content.ReadAsStringAsync();
        BearerToken token = JsonSerializer.Deserialize<BearerToken>(content);

        return token;
    }

    private async Task HandleHttpResponseAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var statusCode = response.StatusCode;
            Log.Error("Error response received from {S} | statusCode: {StatusCode}",
                      response.RequestMessage.RequestUri.ToString(), statusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            throw new
                IdentityProvider($"An IdentityProviderClienIdentityProvidernse: {responseContent}");
        }
    }
}