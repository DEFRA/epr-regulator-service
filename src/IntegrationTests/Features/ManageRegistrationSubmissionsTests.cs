using System.Net;
using System.Text.Json;

using Xunit;
using AwesomeAssertions;
using AwesomeAssertions.Execution;

using IntegrationTests.Infrastructure;
using IntegrationTests.PageModels;

using MockRegulatorFacade.FacadeApi;

using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

[Collection("Sequential")] // Shared mock facade can't be used safely in parallel
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
            CreateSubmission(orgRef: "100001", orgName: "Compliance Scheme Ltd", orgType: "compliance",
                submissionDate: "2024-03-10T09:00:00Z"),
            CreateSubmission(orgRef: "100002", orgName: "Large Producer Corp", orgType: "large",
                submissionDate: "2024-03-11T10:00:00Z"),
            CreateSubmission(orgRef: "100003", orgName: "Small Producer Ltd", orgType: "small",
                submissionDate: "2024-03-12T11:00:00Z"),
        ]);

        // Act
        var response = await Client.GetAsync("/regulators/manage-registration-submissions");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var registrationSubmissionsPage =
            new ManageRegistrationSubmissionsPageModel(await response.Content.ReadAsStringAsync());

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

    private void SetupUserAccountsMock() =>
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

    [Fact]
    public async Task ShowsOrganisationDetailFromFacade()
    {
        // Arrange
        var submissionId = Guid.Parse("0163A629-7780-445F-B00E-1898546BDF0C");

        // Use the JSON file from MockRegulatorFacade as the response
        FacadeServer.WithFacadeApi(submissionId);

        // Act
        var response = await Client.GetAsync($"/regulators/registration-submission-details/{submissionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var htmlContent = await response.Content.ReadAsStringAsync();
        var detailsPage = new ManageRegistrationSubmissionDetailsPageModel(htmlContent);

        using (new AssertionScope())
        {
            detailsPage.OrganisationName.Should().NotBeNull("Organisation name should be displayed on the page");
            detailsPage.OrganisationName.Should().Be("Compliance Scheme Ltd");

            detailsPage.OrganisationType.Should().NotBeNull("Organisation type should be displayed on the page");
            detailsPage.OrganisationType.Should().Contain("Compliance Scheme");

            detailsPage.RelevantYear.Should().Be(2025, "Relevant year should match the facade response");

            // Note: RegistrationJourneyType is not rendered in the HTML and cannot be validated through integration tests.
            // It's used in the ViewModel to control behavior (e.g., payment details component logic).
            // The mapping from facade to ViewModel should be validated through unit tests.
            // The successful page rendering confirms the ViewModel was populated correctly from the facade response.

            // Validate submissionId appears in the page content (it may be in forms, components, or URLs)
            // The successful 200 response already validates the correct submissionId was used in the route
            htmlContent.Should().Contain(submissionId.ToString(), "SubmissionId should appear somewhere in the page content");
        }
    }

}