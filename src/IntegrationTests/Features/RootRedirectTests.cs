namespace IntegrationTests.Features;

using AwesomeAssertions;
using Infrastructure;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

public class RootRedirectTests : IntegrationTestBase
{
    [Fact]
    public async Task RootPath_RedirectsToRegulators()
    {
        var response = await Client.GetAsync("/");
        response.RequestMessage.RequestUri.AbsolutePath.Should().Be("/regulators");
    }

    [Fact]
    public async Task HomePage_WhenRedirectIndexToConfigured_RedirectsToConfiguredUrl()
    {
        // Arrange
        const string cdpIndexUrl = "https://regulator.cdp.example/home";
        using var factory = Factory.WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["REDIRECT_INDEX_TO"] = cdpIndexUrl,
                })));
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        // Act
        var response = await client.GetAsync("/regulators/home");

        // Assert
        response.Headers.Location.Should().Be(cdpIndexUrl);
    }
}
