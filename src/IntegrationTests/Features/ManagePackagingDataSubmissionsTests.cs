namespace IntegrationTests.Features;

using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Infrastructure;
using PageModels;

public class ManageRegistrationSubmissionsTests : IntegrationTestBase
{
    [Fact]
    public async Task ShowsCorrectHeadings()
    {
        // Act
        var response = await Client.GetAsync("/regulators/manage-registration-submissions");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var registrationSubmissionsPage = new ManageRegistrationSubmissionsPageModel(await response.Content.ReadAsStringAsync());
        using (new AssertionScope())
        {
            // Assert on the agency name caption
            registrationSubmissionsPage.AgencyNameCaption.Should().NotBeNull()
                .And.Contain("Environment Agency");

            // Assert on the page heading
            registrationSubmissionsPage.PageHeading.Should().NotBeNull()
                .And.Contain("Manage registration submissions");
        }
    }

    [Fact]
    public async Task ShowsRegistrationSubmissionFromMockFacade()
    {
        // Act
        var response = await Client.GetAsync("/regulators/manage-registration-submissions");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var registrationSubmissionsPage = new ManageRegistrationSubmissionsPageModel(await response.Content.ReadAsStringAsync());
        var tableRows = registrationSubmissionsPage.GetTableRows().ToList();
        tableRows.Should().HaveCount(1);

        using (new AssertionScope())
        {
            var firstRow = tableRows[0];
            firstRow.OrganisationName.Should().Be("Test Company Ltd");
            firstRow.OrganisationType.Should().Contain("Compliance Scheme");
            firstRow.OrganisationReference.Should().Be("100000");
            firstRow.ApplicationDate.Should().Contain("10 January 2025");
            firstRow.Year.Should().Be("2025");
        }
    }
}
