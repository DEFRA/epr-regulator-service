namespace IntegrationTests.Features;

using System.Text.Json;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using IntegrationTests.Infrastructure;
using IntegrationTests.PageModels;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

[Collection(SequentialCollection.Sequential)]
public class ManageRegistrationSubmissionsTests : IntegrationTestBase
{
    public override Task InitializeAsync()
    {
        base.InitializeAsync();
        SetupUserAccountsMock();

        return Task.CompletedTask;
    }

    [Fact]
    public async Task ShowsCorrectHeadings()
    {
        // Arrange
        SetupFacadeMockRegistrationSubmissions([]);

        // Act
        var registrationSubmissionsPage = await GetAsPageModel<ManageRegistrationSubmissionsPageModel>(
            requestUri: "/regulators/manage-registration-submissions");

        // Assert
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
    public async Task ShowsAllRegistrationJourneyTypesFromFacade()
    {
        // Arrange
        SetupFacadeMockRegistrationSubmissions([
            CreateSubmission(orgRef: "100001", orgName: "Compliance Scheme Ltd", orgType: "compliance", journeyType: "CsoLegacy", submissionDate: "2024-03-10T09:00:00Z"),
            CreateSubmission(orgRef: "100002", orgName: "Large Producer Corp", orgType: "large", journeyType: "DirectLargeProducer", submissionDate: "2024-03-11T10:00:00Z"),
            CreateSubmission(orgRef: "100003", orgName: "Small Producer Ltd", orgType: "small", journeyType: "DirectSmallProducer", submissionDate: "2024-03-12T11:00:00Z"),
            CreateSubmission(orgRef: "100004", orgName: "CSO Large Producer Ltd", orgType: "compliance", journeyType: "CsoLargeProducer", submissionDate: "2024-03-13T12:00:00Z"),
            CreateSubmission(orgRef: "100005", orgName: "CSO Small Producer Ltd", orgType: "compliance", journeyType: "CsoSmallProducer", submissionDate: "2024-03-14T13:00:00Z"),
        ]);

        // Act
        var registrationSubmissionsPage = await GetAsPageModel<ManageRegistrationSubmissionsPageModel>(
            requestUri: "/regulators/manage-registration-submissions");

        // Assert
        var tableRows = registrationSubmissionsPage.GetTableRows().ToList();
        tableRows.Should().HaveCount(5);

        using (new AssertionScope())
        {
            tableRows[0].OrganisationName.Should().Be("Compliance Scheme Ltd");
            tableRows[0].OrganisationType.Should().Be("Compliance scheme");
            tableRows[0].OrganisationReference.Should().Be("100001");

            tableRows[1].OrganisationName.Should().Be("Large Producer Corp");
            tableRows[1].OrganisationType.Should().Be("Large producer");
            tableRows[1].OrganisationReference.Should().Be("100002");

            tableRows[2].OrganisationName.Should().Be("Small Producer Ltd");
            tableRows[2].OrganisationType.Should().Be("Small producer");
            tableRows[2].OrganisationReference.Should().Be("100003");

            tableRows[3].OrganisationName.Should().Be("CSO Large Producer Ltd");
            tableRows[3].OrganisationType.Should().Be("Compliance scheme: large producer");
            tableRows[3].OrganisationReference.Should().Be("100004");

            tableRows[4].OrganisationName.Should().Be("CSO Small Producer Ltd");
            tableRows[4].OrganisationType.Should().Be("Compliance scheme: small producer");
            tableRows[4].OrganisationReference.Should().Be("100005");
        }
    }

    private static object CreateSubmission(string orgRef, string orgName, string orgType, string journeyType, string submissionDate) =>
        new
        {
            submissionId = Guid.NewGuid().ToString(),
            organisationId = Guid.NewGuid().ToString(),
            organisationReference = orgRef,
            organisationName = orgName,
            organisationType = orgType,
            registrationJourneyType = journeyType,
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

    private void SetupFacadeMockRegistrationSubmissions(object[] data)
    {
        FacadeServer.Given(Request.Create()
                .UsingPost()
                .WithPath("/api/organisation-registration-submissions"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new
                {
                    items = data, currentPage = 1, pageSize = 20, totalItems = data.Length,
                })));
    }
}