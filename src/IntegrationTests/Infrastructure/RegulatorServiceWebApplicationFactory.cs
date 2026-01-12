namespace IntegrationTests.Infrastructure;

using EPR.RegulatorService;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;

using WireMock.Server;

public class RegulatorServiceWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly WireMockServer _facadeServer = MockRegulatorFacade.MockRegulatorFacadeServer.Start();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["EprAuthorizationConfig:SignInRedirect"] = "",
            });
        });

        Environment.SetEnvironmentVariable("FacadeApi__BaseUrl", _facadeServer.Url!);
        Environment.SetEnvironmentVariable("EprAuthorizationConfig__FacadeBaseUrl", _facadeServer.Url!);
        Environment.SetEnvironmentVariable("UseLocalSession", "true");
        Environment.SetEnvironmentVariable("FeatureManagement__ManageRegistrationSubmissions", "true");

        builder.ConfigureTestServices(services =>
        {
            // Remove the existing authentication services
            var authDescriptors = services.Where(d => d.ServiceType.FullName?.Contains("Authentication") == true).ToList();
            foreach (var descriptor in authDescriptors)
            {
                services.Remove(descriptor);
            }

            // Replace with test authentication handler - include Cookie scheme for authorization policy
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { })
                .AddCookie(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);

            services.AddSingleton<ITokenAcquisition, MockTokenAcquisition>();

            services.AddAntiforgery(options =>
            {
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.None; // Configure antiforgery to not require HTTPS for testing
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _facadeServer?.Stop();
            _facadeServer?.Dispose();
        }
        base.Dispose(disposing);
    }
}
