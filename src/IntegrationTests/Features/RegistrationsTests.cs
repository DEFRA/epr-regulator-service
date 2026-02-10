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
public class RegistrationsTests : IntegrationTestBase
{
    [Theory]
    [InlineData(null, "Ignored user org name", 1, "Environment Agency", "Manage organisation details submissions")]
    [InlineData("cy", "Ignored user org name", 1, "Asiantaeth yr Amgylchedd", "Rheoli cyflwyniadau manylion sefydliadau")]
    [InlineData(null, "Ignored user org name", 4, "Natural Resources Wales", "Manage organisation details submissions")]
    [InlineData("cy", "Ignored user org name", 4, "Cyfoeth Naturiol Cymru", "Rheoli cyflwyniadau manylion sefydliadau")]
    public async Task RegistrationsPage_ShowsDynamicOrganisationCaption(
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
        SetupFacadeMockRegistrationSubmissions([]);
        await SetLanguage(culture);

        // Act
        var page = await GetAsPageModel<RegistrationsPageModel>(
            requestUri: "/regulators/manage-registrations");

        // Assert
        using (new AssertionScope())
        {
            page.AgencyNameCaption.Should().Be(expectedCaption);
            page.PageHeading.Should().Be(expectedHeading);
        }
    }

    private void SetupFacadeMockRegistrationSubmissions(object[] data) =>
        FacadeServer.Given(Request.Create()
                .UsingGet()
                .WithPath("/api/registrations/get-submissions"))
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
