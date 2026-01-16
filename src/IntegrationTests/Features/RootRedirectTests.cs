namespace IntegrationTests.Features;

using AwesomeAssertions;
using Infrastructure;

public class RootRedirectTests : IntegrationTestBase
{
    [Fact]
    public async Task RootPath_RedirectsToRegulators()
    {
        var response = await Client.GetAsync("/");
        response.RequestMessage.RequestUri.AbsolutePath.Should().Be("/regulators");
    }
}
