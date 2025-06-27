using BradescoGateway.Interfaces;
using BradescoGateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace BradescoGateway.Controllers;
[Route("api/v1/cobranca")]
[ApiController]
public class CobrancaController : ControllerBase
{
    private readonly ILogger<CobrancaController> _logger;
    private readonly IAuthBradescoService _tokenCacheProxy;
    private readonly BoletoService _boletoService;

    public CobrancaController(ILogger<CobrancaController> logger, IAuthBradescoService tokenCacheProxy, BoletoService boletoService)
    {
        _logger = logger;
        _tokenCacheProxy = tokenCacheProxy;
        _boletoService = boletoService;
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        _logger.LogInformation("CobrancaController status endpoint called.");
        return Ok(new { Status = "Cobranca API is running" });
    }

    [HttpGet("token")]
    public async Task<IActionResult> GetAuthToken()
    {
        try
        {
            _logger.LogInformation("Retrieving auth token.");
            DTOs.TokenResponse tokenResponse = await _tokenCacheProxy.GetAuthToken();
            return Ok(tokenResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving auth token.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the auth token.");
        }
    }

    [HttpPost("boleto")]
    public async Task<IActionResult> RegistrarBoletoAsync([FromBody] object payload)
    {
        try
        {
            _logger.LogInformation("Creating boleto with payload: {Payload}", payload);
            IActionResult result = await _boletoService.RegistrarBoletoAsync(payload);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating boleto.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the boleto.");
        }
    }

    [HttpPut("alterar-boleto")]
    public async Task<IActionResult> AlterarBoleto([FromBody] object payload)
    {
        try
        {             
            _logger.LogInformation("Updating boleto with payload: {Payload}", payload);
            IActionResult result = await _boletoService.AlterarBoletoAsync(payload);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating boleto.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the boleto.");
        }
    }
}
