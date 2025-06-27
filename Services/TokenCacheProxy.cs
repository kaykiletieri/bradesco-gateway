using BradescoGateway.DTOs;
using BradescoGateway.Interfaces;

namespace BradescoGateway.Services;

public class TokenCacheProxy : IAuthBradescoService
{
    private readonly CobrancaAuthService _authService;
    private readonly ILogger<TokenCacheProxy> _logger;

    private TokenResponse? _cachedToken;
    private DateTime _tokenExpirationTime;

    public TokenCacheProxy(CobrancaAuthService authService, ILogger<TokenCacheProxy> logger)
    {
        _authService = authService;
        _logger = logger;
        _cachedToken = null;
        _tokenExpirationTime = DateTime.MinValue;
    }

    public async Task<TokenResponse> GetAuthToken()
    {
        if (_cachedToken != null && DateTime.UtcNow < _tokenExpirationTime)
        {
            _logger.LogInformation("Returning cached token.");
            return _cachedToken;
        }

        _logger.LogInformation("Token expired or not cached. Retrieving new token...");
        _cachedToken = await _authService.GetAuthToken();

        _tokenExpirationTime = DateTime.UtcNow.AddSeconds(_cachedToken.GetExpiresInAsInt());

        _logger.LogInformation("New token retrieved and cached. Expires at: {ExpirationTime}", _tokenExpirationTime);
        return _cachedToken;
    }
}
