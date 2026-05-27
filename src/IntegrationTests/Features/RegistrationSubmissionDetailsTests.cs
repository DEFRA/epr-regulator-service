namespace IntegrationTests.Features;

using System.Text.Json;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Builders;
using Infrastructure;
using PageModels;
using WireMock.AwesomeAssertions;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

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

        SetupFacadeMockRegistrationSubmissionDetails(
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

        SetupFacadeMockRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .WithOrganisationName("CSO Large Producer Ltd")
                .WithOrganisationType("compliance")
                .WithRelevantYear(2025));

        SetupPaymentFacadeMockComplianceSchemeRegistrationFee(
            CompliancePaymentResponseBuilder.Default()
                .WithComplianceSchemeRegistrationFee(262000)
                .WithTotalFee(285400)
                .WithPreviousPayment(50000)
                .WithOutstandingPayment(235400));

        // Act
        var detailsPage = await GetAsPageModel<ManageRegistrationSubmissionDetailsPageModel>(
            requestUri: $"/regulators/registration-submission-details/{submissionId}");

        // Assert
        using (new AssertionScope())
        {
            detailsPage.PaymentDetails.Should().NotBeNull();
            detailsPage.PaymentDetails!.HasPaymentSection.Should().BeTrue();
            detailsPage.ApplicationFee.Should().Be(2620.00m);
            detailsPage.SubTotal.Should().Be(2854.00m);
            detailsPage.PreviousPaymentReceived.Should().Be(500.00m);
            detailsPage.TotalOutstanding.Should().Be(2354.00m);
        }
    }

    [Fact]
    public async Task RegistrationSubmissionDetailsPage_PostsComplianceSchemeRegistrationFeeWithClosedLoopRecyclingFields()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        const string appRef = "REG-INT-CSO-CL-001";

        SetupFacadeMockRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .WithOrganisationType("compliance")
                .WithRegistrationJourneyType("CsoLargeProducer")
                .WithApplicationReferenceNumber(appRef)
                .WithCsoMemberClosedLoopRecycling(8));

        SetupPaymentFacadeMockComplianceSchemeRegistrationFee(
            CompliancePaymentResponseBuilder.Default()
                .WithComplianceSchemeRegistrationFee(100000)
                .WithTotalFee(100000));

        // Act
        await GetAsPageModel<ManageRegistrationSubmissionDetailsPageModel>(
            $"/regulators/registration-submission-details/{submissionId}");

        // Assert
        FacadeServer.Should()
            .HaveReceivedACall()
            .UsingPost().And
            .AtPath("/compliance-scheme/registration-fee").And
            .WithBodyAsJson(new JsonPartialMatcher(new
            {
                applicationReferenceNumber = appRef,
                complianceSchemeMembers = new[]
                {
                    new
                    {
                        memberType = "large",
                        isClosedLoopRecycling = true,
                        noOfSubsidiariesClosedLoopRecycling = 8,
                    },
                },
            }));
    }

    [Fact]
    public async Task RegistrationSubmissionDetailsPage_PostsProducerRegistrationFeeWithClosedLoopRecyclingFields()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        const string appRef = "REG-INT-DIRECT-CL-001";

        SetupFacadeMockRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .AsDirectLargeProducer()
                .WithApplicationReferenceNumber(appRef)
                .WithProducerClosedLoopRecycling(5));

        SetupPaymentFacadeMockProducerRegistrationFee();

        // Act
        await GetAsPageModel<ManageRegistrationSubmissionDetailsPageModel>(
            $"/regulators/registration-submission-details/{submissionId}");

        // Assert
        FacadeServer.Should()
            .HaveReceivedACall()
            .UsingPost().And
            .AtPath("/producer/registration-fee").And
            .WithBodyAsJson(new JsonPartialMatcher(new
            {
                applicationReferenceNumber = appRef,
                producerType = "large",
                isClosedLoopRecycling = true,
                noOfSubsidiariesClosedLoopRecycling = 5,
            }));
    }

    private void SetupPaymentFacadeMockComplianceSchemeRegistrationFee(CompliancePaymentResponseBuilder builder) =>
        FacadeServer.Given(Request.Create()
                .UsingPost()
                .WithPath("/compliance-scheme/registration-fee"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(builder.Build())));

    private void SetupPaymentFacadeMockProducerRegistrationFee() =>
        FacadeServer.Given(Request.Create()
                .UsingPost()
                .WithPath("/producer/registration-fee"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new
                {
                    producerRegistrationFee = 165800m,
                    producerLateRegistrationFee = 0m,
                    producerOnlineMarketPlaceFee = 0m,
                    producerClosedLoopRecyclingFee = 0m,
                    previousPayment = 0m,
                    subsidiariesFee = 0m,
                    totalFee = 165800m,
                    outstandingPayment = 165800m,
                    subsidiariesFeeBreakdown = new
                    {
                        totalSubsidiariesOMPFees = 0m,
                        countOfOMPSubsidiaries = 0
                    }
                })));

    private void SetupFacadeMockRegistrationSubmissionDetails(RegistrationSubmissionDetailsBuilder builder) =>
        FacadeServer.Given(Request.Create()
                .UsingGet()
                .WithPath($"/api/organisation-registration-submission-details/{builder.SubmissionId}"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(builder.Build())));
}
