namespace IntegrationTests.Infrastructure;

using WireMock.Server;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private RegulatorServiceWebApplicationFactory Factory { get; set; } = null!;
    protected HttpClient Client { get; private set; } = null!;
    protected WireMockServer FacadeServer => Factory.FacadeServer;

    public Task InitializeAsync()
    {
        Factory = new RegulatorServiceWebApplicationFactory();
        Client = Factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
    }
}
