using BradescoGateway.DTOs;
using BradescoGateway.Interfaces;
using System.Text.Json;

namespace BradescoGateway.Services;

public class CobrancaAuthService : IAuthBradescoService
{
    private static readonly JsonSerializerOptions CachedJsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ILogger<CobrancaAuthService> _logger;
    private readonly HttpClient _httpClient;

    public CobrancaAuthService(ILogger<CobrancaAuthService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }
    public async Task<TokenResponse> GetAuthToken()
    {
        try
        {
            _logger.LogDebug("Starting to get the cobranca auth token.");
            HttpRequestMessage request = new(HttpMethod.Post, "auth/server-mtls/v2/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", Environment.GetEnvironmentVariable("CLIENT_ID") ?? throw new InvalidOperationException("Environment variable 'CLIENT_ID' is not set.") },
                    { "client_secret", Environment.GetEnvironmentVariable("CLIENT_SECRET") ?? throw new InvalidOperationException("Environment variable 'CLIENT_SECRET' is not set.") }
                })
            };

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Cobranca auth token retrieved successfully: {Content}", content);

                return JsonSerializer.Deserialize<TokenResponse>(content, CachedJsonSerializerOptions)
                       ?? throw new JsonException("Failed to deserialize the token response.");
            }
            else
            {
                _logger.LogError("Failed to retrieve cobranca auth token. Status code: {StatusCode}, Reason: {ReasonPhrase}",
                    response.StatusCode, response.ReasonPhrase);
                throw new HttpRequestException($"Failed to retrieve cobranca auth token. Status code: {response.StatusCode}");
            }
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "Error while making HTTP request to get the auth token.");
            throw;
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "Error deserializing the auth token response.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Inexpected error while getting the auth token.");
            throw;
        }
    }
}
