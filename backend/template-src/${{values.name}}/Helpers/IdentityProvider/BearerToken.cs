using System.Text.Json.Serialization;

namespace RestService.Helpers.IdentityProvider;

/// <summary>
/// Represents the properties defined by a bearer token.
/// </summary>
public class BearerToken
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; }

    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_expires_in")]
    public int RefreshExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")] public string RefreshToken { get; set; }

    [JsonPropertyName("token_type")] public string TokenType { get; set; }

    [JsonPropertyName("not-before-policy")]
    public int NotBeforePolicy { get; set; } // Change the data type to int

    [JsonPropertyName("session_state")] public string SessionState { get; set; }

    [JsonPropertyName("scope")] public string Scope { get; set; }
}