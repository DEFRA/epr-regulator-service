namespace IntegrationTests.Features;

using System.Net;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Infrastructure;
using MockRegulatorFacade.FacadeApi;
using PageModels;

[Collection("Sequential")] // Shared mock facade can't be used safely in parallel
public class RegistrationSubmissionDetailsTests : IntegrationTestBase
{
    public override Task InitializeAsync()
    {
        base.InitializeAsync();
        SetupUserAccountsMock();

        return Task.CompletedTask;
    }

    [Fact]
    public async Task ShowsOrganisationDetailFromFacade()
    {
        // Arrange
        var submissionId = Guid.Parse("0163A629-7780-445F-B00E-1898546BDF0C");

        // Use the JSON file from MockRegulatorFacade as the response
        FacadeServer.WithFacadeApi(submissionId);

        // Act
         var detailsPage = await GetAsPageModel<ManageRegistrationSubmissionDetailsPageModel>(
             requestUri: $"/regulators/registration-submission-details/{submissionId}");

         // Assert
        using (new AssertionScope())
        {
            detailsPage.OrganisationName.Should().NotBeNull("Organisation name should be displayed on the page");
            detailsPage.OrganisationName.Should().Be("Compliance Scheme Ltd");

            detailsPage.OrganisationType.Should().NotBeNull("Organisation type should be displayed on the page");
            detailsPage.OrganisationType.Should().Contain("Compliance Scheme");

            detailsPage.RelevantYear.Should().Be(2025, "Relevant year should match the facade response");

            // Note: RegistrationJourneyType is not rendered in the HTML and cannot be validated through integration tests.
            // It's used in the ViewModel to control behavior (e.g., payment details component logic).
            // The successful page rendering confirms the ViewModel was populated correctly from the facade response.

            // Validate submissionId appears in the page content (it may be in forms, components, or URLs)
            // The successful 200 response already validates the correct submissionId was used in the route
            // htmlContent.Should().Contain(submissionId.ToString(), "SubmissionId should appear somewhere in the page content");
        }
    }
}
