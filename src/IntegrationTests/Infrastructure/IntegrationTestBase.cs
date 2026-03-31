namespace IntegrationTests.Infrastructure;

using System.Text.Json;

using IntegrationTests.Builders;
using Microsoft.AspNetCore.Mvc.Testing;
using PageModels;

using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected RegulatorServiceWebApplicationFactory Factory { get; private set; } = null!;
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

    /// <summary>HTTP client that does not follow redirects (e.g. assert 302 Location for form posts).</summary>
    protected HttpClient CreateNoRedirectClient() =>
        Factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    protected void SetupUserAccountsMock() =>
        SetupUserAccountsMock(UserAccountBuilder.Default());

    protected void SetupUserAccountsMock(UserAccountBuilder builder) =>
        FacadeServer.Given(Request.Create()
                .UsingGet()
                .WithPath("/api/user-accounts"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(builder.Build())));

    /// <summary>
    /// Simulate user clicking english/welsh links at top of page
    /// </summary>
    protected async Task SetLanguage(string? culture)
    {
        if (culture == null)
        {
            return;
        }

        await Client.GetAsync($"/regulators/culture?culture={culture}&returnUrl=/regulators/home");
    }

    protected async Task<TPageModel> GetAsPageModel<TPageModel>(string? requestUri)
        where TPageModel : PageModelBase, IPageModelFactory<TPageModel>
    {
        var response = await Client.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();
        var htmlContent = await response.Content.ReadAsStringAsync();
        return TPageModel.FromContent(htmlContent);
    }
}
