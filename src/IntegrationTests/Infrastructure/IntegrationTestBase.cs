namespace IntegrationTests.Infrastructure;

using System.Text.Json;

using PageModels;

using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
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

    protected void SetupUserAccountsMock() =>
        FacadeServer.Given(Request.Create()
                .UsingGet()
                .WithPath("/api/user-accounts"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new
                {
                    user = new
                    {
                        id = "62309b0e-535d-4f96-9a3b-9c759a3944f3",
                        firstName = "Test",
                        lastName = "User",
                        email = "test.user@example.com",
                        roleInOrganisation = "Admin",
                        enrolmentStatus = "Approved",
                        serviceRole = "Regulator Basic",
                        service = "RegulatorService",
                        serviceRoleId = 5,
                        organisations = new[]
                        {
                            new
                            {
                                id = "C7646CAE-EB96-48AC-9427-0120199BE6EE",
                                name = "Environment Agency",
                                organisationRole = "Regulator",
                                organisationType = "Regulators",
                                nationId = 1,
                            },
                        },
                    },
                })));

    protected async Task<TPageModel> GetAsPageModel<TPageModel>(string? requestUri)
        where TPageModel : PageModelBase, IPageModelFactory<TPageModel>
    {
        var response = await Client.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();
        var htmlContent = await response.Content.ReadAsStringAsync();
        return TPageModel.FromContent(htmlContent);
    }
}
