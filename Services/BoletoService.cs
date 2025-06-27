using BradescoGateway.DTOs;
using BradescoGateway.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace BradescoGateway.Services;

public class BoletoService
{
    private readonly IAuthBradescoService _authService;
    private readonly HttpClient _httpClient;
    private readonly ILogger<BoletoService> _logger;

    public BoletoService(IAuthBradescoService authService, HttpClient httpClient, ILogger<BoletoService> logger)
    {
        _authService = authService;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IActionResult> RegistrarBoletoAsync(object payload)
    {
        try
        {
            TokenResponse tokenResponse = await _authService.GetAuthToken();
            string accessToken = tokenResponse.AccessToken;

            string jsonPayload = JsonSerializer.Serialize(payload);
            StringContent content = new(jsonPayload, Encoding.UTF8, "application/json");

            HttpRequestMessage requestMessage = new(HttpMethod.Post, "boleto/cobranca-registro/v1/cobranca")
            {
                Content = content
            };

            requestMessage.Headers.Add("Authorization", $"Bearer {accessToken}");

            HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Boleto registrado com sucesso: {ResponseContent}", responseContent);
                return new JsonResult(new { response.StatusCode, Message = responseContent });
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Falha ao registrar boleto. Status code: {StatusCode}, Mensagem: {ErrorContent}",
                    response.StatusCode, errorContent);
                return new JsonResult(new { response.StatusCode, Message = errorContent });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar boleto.");
            return new JsonResult(new { StatusCode = 500, Message = "Erro interno no servidor." });
        }
    }

    public async Task<IActionResult> AlterarBoletoAsync(object payload)
    {
        try
        {
            TokenResponse tokenResponse = await _authService.GetAuthToken();
            string accessToken = tokenResponse.AccessToken;

            string jsonPayload = JsonSerializer.Serialize(payload);
            StringContent content = new(jsonPayload, Encoding.UTF8, "application/json");

            HttpRequestMessage requestMessage = new(HttpMethod.Put, "boleto/cobranca-altera/v1/alterar")
            {
                Content = content
            };

            requestMessage.Headers.Add("Authorization", $"Bearer {accessToken}");
            
            HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Boleto alterado com sucesso. Resposta: {ResponseContent}", responseContent);
                return new JsonResult(new { response.StatusCode, Message = responseContent });
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Falha ao alterar boleto. Status code: {StatusCode}, Mensagem: {ErrorContent}",
                    response.StatusCode, errorContent);
                return new JsonResult(new { response.StatusCode, Message = errorContent });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao alterar boleto.");
            return new JsonResult(new { StatusCode = 500, Message = "Erro interno no servidor." });
        }
    }
}
