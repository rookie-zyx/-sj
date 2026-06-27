using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Pharmaceutical.WebAPI.Auth;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "ApiKey";
    public const string HeaderName = "X-Api-Key";

    private readonly IConfiguration _configuration;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration configuration)
        : base(options, logger, encoder)
    {
        _configuration = configuration;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var configuredKey = _configuration["ApiSettings:ApiKey"];
        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            Logger.LogWarning("ApiSettings:ApiKey 未配置，API 认证已跳过");
            var anonymousIdentity = new ClaimsIdentity(Array.Empty<Claim>(), SchemeName);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(
                new ClaimsPrincipal(anonymousIdentity), SchemeName)));
        }

        if (!Request.Headers.TryGetValue(HeaderName, out var providedKey)
            || string.IsNullOrWhiteSpace(providedKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("缺少 API Key"));
        }

        if (!string.Equals(providedKey, configuredKey, StringComparison.Ordinal))
        {
            return Task.FromResult(AuthenticateResult.Fail("API Key 无效"));
        }

        var claims = new[] { new Claim(ClaimTypes.Name, "ApiClient") };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
