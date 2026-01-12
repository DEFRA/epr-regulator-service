namespace IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private RegulatorServiceWebApplicationFactory Factory { get; set; } = null!;
    protected HttpClient Client { get; private set; } = null!;

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
