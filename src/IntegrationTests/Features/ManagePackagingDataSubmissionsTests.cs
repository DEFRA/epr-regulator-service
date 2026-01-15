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
    public async Task ShowsAllOrganisationTypesFromFacade()
    {
        // Arrange
        SetupFacadeMockRegistrationSubmissions([
            CreateSubmission(orgRef: "100001", orgName: "Compliance Scheme Ltd", orgType: "compliance", submissionDate: "2024-03-10T09:00:00Z"),
            CreateSubmission(orgRef: "100002", orgName: "Large Producer Corp", orgType: "large", submissionDate: "2024-03-11T10:00:00Z"),
            CreateSubmission(orgRef: "100003", orgName: "Small Producer Ltd", orgType: "small", submissionDate: "2024-03-12T11:00:00Z"),
        ]);

        // Act
        var response = await Client.GetAsync("/regulators/manage-registration-submissions");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var registrationSubmissionsPage = new ManageRegistrationSubmissionsPageModel(await response.Content.ReadAsStringAsync());

        var tableRows = registrationSubmissionsPage.GetTableRows().ToList();
        tableRows.Should().HaveCount(3);

        using (new AssertionScope())
        {
            tableRows[0].OrganisationName.Should().Be("Compliance Scheme Ltd");
            tableRows[0].OrganisationType.Should().Contain("Compliance Scheme");
            tableRows[0].OrganisationReference.Should().Be("100001");
            tableRows[0].ApplicationDate.Should().Contain("10 March 2024");

            tableRows[1].OrganisationName.Should().Be("Large Producer Corp");
            tableRows[1].OrganisationType.Should().Contain("Large Producer");
            tableRows[1].OrganisationReference.Should().Be("100002");
            tableRows[1].ApplicationDate.Should().Contain("11 March 2024");

            tableRows[2].OrganisationName.Should().Be("Small Producer Ltd");
            tableRows[2].OrganisationType.Should().Contain("Small Producer");
            tableRows[2].OrganisationReference.Should().Be("100003");
            tableRows[2].ApplicationDate.Should().Contain("12 March 2024");
        }
    }

    private static object CreateSubmission(string orgRef, string orgName, string orgType, string submissionDate) =>
        new
        {
            submissionId = Guid.NewGuid().ToString(),
            organisationId = Guid.NewGuid().ToString(),
            organisationReference = orgRef,
            organisationName = orgName,
            organisationType = orgType,
            applicationReferenceNumber = $"REG-2024-{orgRef}",
            registrationReferenceNumber = (string?)null,
            submissionDate,
            registrationYear = 2024,
            nationId = 1,
            submissionStatus = "Pending",
            resubmissionStatus = (string?)null,
            statusPendingDate = submissionDate,
            isResubmission = false,
            resubmissionFileId = (string?)null,
            resubmissionDate = (string?)null,
            registrationDate = (string?)null,
            regulatorDecisionDate = (string?)null,
            resubmissionDecisionDate = (string?)null,
            regulatorCommentDate = (string?)null,
            producerCommentDate = (string?)null,
            regulatorUserId = (string?)null,
        };

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
