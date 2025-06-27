using System.Text.Json.Serialization;

namespace BradescoGateway.DTOs;

public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; private set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; private set; }

    [JsonPropertyName("expires_in")]
    public string ExpiresIn { get; private set; }

    public TokenResponse(string accessToken, string tokenType, string expiresIn)
    {
        AccessToken = accessToken;
        TokenType = tokenType;
        ExpiresIn = expiresIn;
    }

    public int GetExpiresInAsInt()
    {
        return int.TryParse(ExpiresIn, out int expiresIn) ? expiresIn : 0;
    }
}
