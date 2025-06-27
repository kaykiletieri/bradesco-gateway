using Microsoft.Extensions.Primitives;

namespace TechAuthHub.Infrastructure.Middlewares;

public class TokenAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _fixedToken;
    private readonly HashSet<string> _ignoredEndpoints;
    private readonly ILogger<TokenAuthenticationMiddleware> _logger;

    public TokenAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration, IHostEnvironment environment, ILogger<TokenAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;

        _fixedToken = configuration["FIXED_API_KEY"]
            ?? throw new ArgumentNullException(nameof(configuration), "FIXED_API_KEY not found in configuration.");

        _ignoredEndpoints = configuration.GetSection("FixTokenIgnoredEndpoints").Get<HashSet<string>>() ?? [];

        _logger.LogInformation("Authentication middleware configured for environment: {Environment}", environment.EnvironmentName);
        _logger.LogInformation("Ignored Endpoints: {Endpoints}", string.Join(", ", _ignoredEndpoints));
    }

    public async Task Invoke(HttpContext context)
    {
        string? requestPath = context.Request.Path.Value?.ToLower();
        if (_ignoredEndpoints.Contains(requestPath!))
        {
            _logger.LogInformation("Skipping authentication for endpoint: {Path}", requestPath);
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("x-api-key", out StringValues extractedToken) || extractedToken != _fixedToken)
        {
            _logger.LogWarning("Invalid API Key: {ApiKey}", extractedToken!);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized: Invalid API Key.");
            return;
        }

        await _next(context);
    }
}
