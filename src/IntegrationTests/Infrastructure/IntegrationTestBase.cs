namespace IntegrationTests.Infrastructure;

using WireMock.Server;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private RegulatorServiceWebApplicationFactory Factory { get; set; } = null!;
    protected HttpClient Client { get; private set; } = null!;
    protected WireMockServer FacadeServer => Factory.FacadeServer;

    public virtual Task InitializeAsync()
    {
        Factory = new RegulatorServiceWebApplicationFactory();
        Client = Factory.CreateClient();
        FacadeServer.ResetMappings(); // Avoid mystery-guest of json based default mappings. Tests should define their own required data explicitly
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
    }
}
