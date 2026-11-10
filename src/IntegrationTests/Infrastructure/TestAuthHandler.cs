namespace IntegrationTests.Infrastructure;

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Replacement for production auth.
/// See AuthMock.md
/// </summary>
public class TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "TestScheme";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Opt-in: behave like production, where the cookie issued by UserDataCheckerMiddleware
        // carries the user-data claim, so the middleware doesn't reset the session on every request.
        var config = Context.RequestServices.GetRequiredService<IConfiguration>();
        if (config.GetValue<bool>("IntegrationTests:HonourSignInCookie"))
        {
            var cookieResult = await Context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (cookieResult.Succeeded)
            {
                return AuthenticateResult.Success(new AuthenticationTicket(cookieResult.Principal!, SchemeName));
            }
        }

        const string testUserId = "62309B0E-535D-4F96-9A3B-9C759A3944F3";
        var claims = new[]
        {
            new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", testUserId),
        };

        return AuthenticateResult.Success(
            new AuthenticationTicket(
                new ClaimsPrincipal(
                    new ClaimsIdentity(claims, SchemeName)),
                SchemeName));
    }
}
