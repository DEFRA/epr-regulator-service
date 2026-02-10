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
public class SubmissionsTests : IntegrationTestBase
{
    [Theory]
    [InlineData(null, "Ignored user org name", 1, "Environment Agency", "Manage packaging data submissions")]
    [InlineData("cy", "Ignored user org name", 1, "Asiantaeth yr Amgylchedd", "Rheoli cyflwyniadau data pecynwaith")]
    [InlineData(null, "Ignored user org name", 4, "Natural Resources Wales", "Manage packaging data submissions")]
    [InlineData("cy", "Ignored user org name", 4, "Cyfoeth Naturiol Cymru", "Rheoli cyflwyniadau data pecynwaith")]
    public async Task SubmissionsPage_ShowsDynamicOrganisationCaption(
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
        SetupFacadeMockPomSubmissions([]);
        await SetLanguage(culture);

        // Act
        var page = await GetAsPageModel<SubmissionsPageModel>(
            requestUri: "/regulators/manage-packaging-data-submissions");

        // Assert
        using (new AssertionScope())
        {
            page.AgencyNameCaption.Should().Be(expectedCaption);
            page.PageHeading.Should().Be(expectedHeading);
        }
    }

    private void SetupFacadeMockPomSubmissions(object[] data) =>
        FacadeServer.Given(Request.Create()
                .UsingGet()
                .WithPath("/api/pom/get-submissions"))
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
