namespace IntegrationTests.Features;

using System.Text.Json;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Infrastructure;
using PageModels;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

public class ManageRegistrationSubmissionsTests : IntegrationTestBase
{
    [Fact]
    public async Task ShowsCorrectHeadings()
    {
        // Act
        var response = await Client.GetAsync("/regulators/manage-registration-submissions");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var registrationSubmissionsPage =
            new ManageRegistrationSubmissionsPageModel(await response.Content.ReadAsStringAsync());
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
    public async Task ShowsRegistrationSubmissionFromFacade()
    {
        // Arrange
        SetupFacadeMockRegistrationSubmissions([
            new
            {
                submissionId = "A1B2C3D4-E5F6-7890-ABCD-EF1234567890",
                organisationId = "12345678-1234-1234-1234-123456789012",
                organisationReference = "ORG-REF-99",
                organisationName = "Acme Recycling Corp",
                organisationType = "compliance",
                applicationReferenceNumber = "REG-2024-XYZ",
                registrationReferenceNumber = (string?)null,
                submissionDate = "2024-03-15T14:30:00Z",
                registrationYear = 2024,
                nationId = 1,
                submissionStatus = "Pending",
                resubmissionStatus = (string?)null,
                statusPendingDate = "2024-03-16T14:30:00Z",
                isResubmission = false,
                resubmissionFileId = (string?)null,
                resubmissionDate = (string?)null,
                registrationDate = (string?)null,
                regulatorDecisionDate = (string?)null,
                resubmissionDecisionDate = (string?)null,
                regulatorCommentDate = (string?)null,
                producerCommentDate = (string?)null,
                regulatorUserId = (string?)null,
            },
        ]);

        // Act
        var response = await Client.GetAsync("/regulators/manage-registration-submissions");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var registrationSubmissionsPage =
            new ManageRegistrationSubmissionsPageModel(await response.Content.ReadAsStringAsync());
        var tableRows = registrationSubmissionsPage.GetTableRows().ToList();
        tableRows.Should().HaveCount(1);

        using (new AssertionScope())
        {
            var firstRow = tableRows[0];
            firstRow.OrganisationReference.Should().Be("ORG-REF-99");
            firstRow.OrganisationName.Should().Be("Acme Recycling Corp");
            firstRow.OrganisationType.Should().Contain("Compliance Scheme");
            firstRow.ApplicationDate.Should().Contain("15 March 2024");
            firstRow.Year.Should().Be("2024");
        }
    }

    private void SetupFacadeMockRegistrationSubmissions(object[] data) =>
        FacadeServer.Given(Request.Create()
                .UsingPost()
                .WithPath("/api/organisation-registration-submissions"))
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
