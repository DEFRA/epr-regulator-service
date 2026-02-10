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
    [InlineData(null, "Ignored user org name", 2, "Northern Ireland Environment Agency", "Applications for approved and delegated people")]
    [InlineData("cy", "Ignored user org name", 2, "Asiantaeth Amgylchedd Gogledd Iwerddon", "Ceisiadau am bobl a gymeradwywyd a phobl a ddirprwywyd")]
    [InlineData(null, "Ignored user org name", 3, "Scottish Environment Protection Agency", "Applications for approved and delegated people")]
    [InlineData("cy", "Ignored user org name", 3, "Asiantaeth Diogelu Amgylchedd yr Alban", "Ceisiadau am bobl a gymeradwywyd a phobl a ddirprwywyd")]
    [InlineData(null, "Ignored user org name", 4, "Natural Resources Wales", "Applications for approved and delegated people")]
    [InlineData("cy", "Ignored user org name", 4, "Cyfoeth Naturiol Cymru", "Ceisiadau am bobl a gymeradwywyd a phobl a ddirprwywyd")]
    public async Task ApplicationsPage_ShowsDynamicOrganisationCaption(
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
