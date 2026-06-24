namespace IntegrationTests.Features;

using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Builders;
using Infrastructure;
using PageModels;

[Collection(SequentialCollection.Sequential)]
public class RegistrationSubmissionDetailsTests : IntegrationTestBase
{
    public override Task InitializeAsync()
    {
        base.InitializeAsync();
        SetupUserAccountsMock();

        return Task.CompletedTask;
    }

    [Fact]
    public async Task RegistrationSubmissionDetailsPage_ShowsOrganisationDetailsFromFacade()
    {
        // Arrange
        var submissionId = Guid.Parse("0163A629-7780-445F-B00E-1898546BDF0C");

        FacadeServer.SetupRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .WithOrganisationName("Compliance Scheme Ltd")
                .WithOrganisationType("compliance")
                .WithRelevantYear(2025));

        // Act
        var detailsPage = await GetAsPageModel<ManageRegistrationSubmissionDetailsPageModel>(
            requestUri: $"/regulators/registration-submission-details/{submissionId}");

        // Assert
        using (new AssertionScope())
        {
            detailsPage.OrganisationName.Should().Be("Compliance Scheme Ltd");
            detailsPage.OrganisationType.Should().Contain("Compliance Scheme");
            detailsPage.RelevantYear.Should().Be(2025);
        }
    }

    [Fact]
    public async Task RegistrationSubmissionDetailsPage_ShowsComplianceSchemePaymentDetailsFromPaymentFacade()
    {
        // Arrange
        var submissionId = Guid.Parse("1b2c3d4e-5f6a-7b8c-9d0e-1f2a3b4c5d6e");

        FacadeServer.SetupRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .WithOrganisationName("CSO Large Producer Ltd")
                .WithOrganisationType("compliance")
                .WithRelevantYear(2025));

        FacadeServer.SetupComplianceSchemeRegistrationFee(
            CompliancePaymentResponseBuilder.Default()
                .WithComplianceSchemeRegistrationFee(284200)
                .WithTotalFee(588500)
                .WithPreviousPayment(50000)
                .WithOutstandingPayment(538500)
                .WithDefaultMemberFees(38600, 254800, 566300));

        // Act
        var detailsPage = await GetAsPageModel<ManageRegistrationSubmissionDetailsPageModel>(
            requestUri: $"/regulators/registration-submission-details/{submissionId}");

        // Assert
        using (new AssertionScope())
        {
            detailsPage.PaymentDetails.Should().NotBeNull();
            detailsPage.PaymentDetails!.HasPaymentSection.Should().BeTrue();
            detailsPage.ApplicationFee.Should().Be(2842.00m);
            detailsPage.SubTotal.Should().Be(5885.00m);
            detailsPage.PreviousPaymentReceived.Should().Be(500.00m);
            detailsPage.TotalOutstanding.Should().Be(5385.00m);
        }
    }
}
