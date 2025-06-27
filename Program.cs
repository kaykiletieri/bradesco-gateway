using BradescoGateway.Interfaces;
using BradescoGateway.Services;
using Polly;
using System.Security.Cryptography.X509Certificates;
using TechAuthHub.Infrastructure.Middlewares;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string? certPath = Environment.GetEnvironmentVariable("CLIENT_CERT_PATH")
    ?? throw new InvalidOperationException("Environment variable 'CLIENT_CERT_PATH' is not set.");
string? certPassword = Environment.GetEnvironmentVariable("CLIENT_CERT_PASSWORD")
    ?? throw new InvalidOperationException("Environment variable 'CLIENT_CERT_PASSWORD' is not set.");
string? apiBaseUrl = Environment.GetEnvironmentVariable("API_BASE_URL")
    ?? throw new InvalidOperationException("Environment variable 'API_BASE_URL' is not set.");

X509Certificate2 certificate = X509CertificateLoader.LoadPkcs12(
    File.ReadAllBytes(certPath),
    certPassword
);

if (certificate.NotAfter < DateTime.Now)
{
    throw new InvalidOperationException("The certificate has expired.");
}

HttpClientHandler handler = new();
handler.ClientCertificates.Add(certificate);

builder.Services.AddHttpClient<CobrancaAuthService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);

})
.ConfigurePrimaryHttpMessageHandler(() => handler)
.SetHandlerLifetime(TimeSpan.FromMinutes(5))
.AddTransientHttpErrorPolicy(policyBuilder =>
    policyBuilder.WaitAndRetryAsync(3, attempt =>
        TimeSpan.FromSeconds(Math.Pow(2, attempt))));

builder.Services.AddSingleton<IAuthBradescoService, TokenCacheProxy>();
builder.Services.AddSingleton<BoletoService>();

builder.Services.AddControllers();

WebApplication app = builder.Build();

app.UseMiddleware<TokenAuthenticationMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
