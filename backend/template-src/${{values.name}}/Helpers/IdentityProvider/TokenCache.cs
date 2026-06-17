using Serilog;

namespace RestService.Helpers.IdentityProvider;

/// <summary>
/// Local not distributed token cache to store tokens.
/// </summary>
public class TokenCache<T>
{
    private static readonly object _instanceLock = new();
    private static TokenCache<T> _instance;

    private BearerToken _token;
    private DateTime _tokenExpiryTime;
    private readonly SemaphoreSlim _tokenRetrievalLock = new(1, 1);

    private TokenCache()
    {
    }

    public static TokenCache<T> GetInstance()
    {
        if (_instance == null)
        {
            lock (_instanceLock)
            {
                if (_instance == null)
                {
                    _instance = new TokenCache<T>();
                }
            }
        }

        return _instance;
    }

    public async Task<BearerToken> GetToken(IIdentityProviderClient client, string username,
        string password)
    {
        if (_token == null || DateTime.Now >= _tokenExpiryTime)
        {
            await _tokenRetrievalLock.WaitAsync();

            try
            {
                if (_token == null || DateTime.Now >= _tokenExpiryTime)
                {
                    Log.Information("Token not set or has expired, fetching new one.");

                    _token = await client.GetToken(username, password);
                    _tokenExpiryTime = DateTime.Now.AddSeconds(_token.ExpiresIn - 10);

                    Log.Information("Token will expire at: {time}", _tokenExpiryTime);
                }
            }
            finally
            {
                _tokenRetrievalLock.Release();
            }
        }

        return _token;
    }
}