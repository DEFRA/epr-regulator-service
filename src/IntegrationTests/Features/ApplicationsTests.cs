namespace IntegrationTests.Features;

using System.Text.Json;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using IntegrationTests.Builders;
using IntegrationTests.Infrastructure;
using IntegrationTests.PageModels;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

[Collection(SequentialCollection.Sequential)]
public class ApplicationsTests : IntegrationTestBase
{
    [Theory]
    [InlineData(null, "Ignored user org name", 1, "Environment Agency", "Applications for approved and delegated people")]
    [InlineData("cy", "Ignored user org name", 1, "Asiantaeth yr Amgylchedd", "Ceisiadau am bobl a gymeradwywyd a phobl a ddirprwywyd")]
    [InlineData(null, "Ignored user org name", 4, "Environment Agency", "Applications for approved and delegated people")] // caption ignores user's org
    [InlineData("cy", "Ignored user org name", 4, "Asiantaeth yr Amgylchedd", "Ceisiadau am bobl a gymeradwywyd a phobl a ddirprwywyd")] // caption ignores user's org
    public async Task ApplicationsPage_ShowsHardcodedEnvironmentAgencyCaption(
        string? culture,
        string organisationName,
        int nationId,
        string expectedCaption,
        string expectedHeading)
    {
        // Arrange
        SetupUserAccountsMock(UserAccountBuilder.Default()
            .WithOrganisationName(organisationName)
            .WithNationId(nationId));
        SetupFacadeMockPendingApplications([]);
        await SetLanguage(culture);

        // Act
        var page = await GetAsPageModel<ApplicationsPageModel>(
            requestUri: "/regulators/applications");

        // Assert
        using (new AssertionScope())
        {
            page.AgencyNameCaption.Should().Be(expectedCaption);
            page.PageHeading.Should().Be(expectedHeading);
        }
    }

    private void SetupFacadeMockPendingApplications(object[] data) =>
        FacadeServer.Given(Request.Create()
                .UsingGet()
                .WithPath("/api/organisations/pending-applications"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new
                {
                    items = data,
                    currentPage = 1,
                    pageSize = 20,
                    totalItems = data.Length,
                })));
}
