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
        var assertions = FacadeServer.Should().HaveReceivedACall();
        assertions
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
                        noOfHoldingCompaniesClosedLoopRecycling = 1,
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

        SetupPaymentFacadeMockProducerRegistrationFee(
            ProducerPaymentResponseBuilder.Default()
                .WithProducerRegistrationFee(100000)
                .WithTotalFee(100000)
                .WithOutstandingPayment(100000));

        // Act
        await GetAsPageModel<ManageRegistrationSubmissionDetailsPageModel>(
            $"/regulators/registration-submission-details/{submissionId}");

        // Assert
        var assertions = FacadeServer.Should().HaveReceivedACall();
        assertions
            .UsingPost().And
            .AtPath("/producer/registration-fee").And
            .WithBodyAsJson(new JsonPartialMatcher(new
            {
                applicationReferenceNumber = appRef,
                producerType = "large",
                noOfHoldingCompaniesClosedLoopRecycling = 1,
                isClosedLoopRecycling = true,
                noOfSubsidiariesClosedLoopRecycling = 5,
            }));
    }

    [Fact]
    public async Task RegistrationSubmissionDetailsPage_ShowsComplianceSchemeSubsidiaryClrPaymentLineItems()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        const int subsidiariesFeePence = 647600;
        const int subsidiaryClrFeePence = 509600;
        const int subsidiaryClrCount = 2;
        const int netSubsidiaryFeePence = subsidiariesFeePence - subsidiaryClrFeePence;

        SetupFacadeMockRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .WithOrganisationType("compliance")
                .WithRegistrationJourneyType("CsoLargeProducer")
                .WithCsoMemberSubsidiaries(subsidiaryClrCount));

        SetupPaymentFacadeMockComplianceSchemeRegistrationFee(
            CompliancePaymentResponseBuilder.Default()
                .WithComplianceSchemeRegistrationFee(284200)
                .WithTotalFee(1174800)
                .WithMembers(ComplianceSchemeMemberFeeBuilder.Default()
                    .WithLateRegistrationFee(77200)
                    .WithTotalMemberFee(890600)
                    .WithSubsidiariesFeeBreakdown(
                        subsidiariesFeePence,
                        subsidiaryClrFeePence,
                        subsidiaryClrCount)));

        // Act
        var detailsPage = await GetAsPageModel<ManageRegistrationSubmissionDetailsPageModel>(
            $"/regulators/registration-submission-details/{submissionId}");

        // Assert
        using (new AssertionScope())
        {
            detailsPage.ApplicationFee.Should().Be(2842.00m);
            detailsPage.SubTotal.Should().Be(11748.00m);

            var subsidiaryCompanies = detailsPage.FindPaymentLineItem("Subsidiary companies");
            subsidiaryCompanies.Should().NotBeNull();
            subsidiaryCompanies!.Units.Should().Be(subsidiaryClrCount);
            subsidiaryCompanies.Amount.Should().Be(netSubsidiaryFeePence / 100m);

            var subsidiaryClr = detailsPage.FindPaymentLineItem("Subsidiaries closed loop packaging waste");
            subsidiaryClr.Should().NotBeNull();
            subsidiaryClr!.Units.Should().Be(subsidiaryClrCount);
            subsidiaryClr.Amount.Should().Be(subsidiaryClrFeePence / 100m);
        }
    }

    [Fact]
    public async Task RegistrationSubmissionDetailsPage_ShowsDirectProducerSubsidiaryClrPaymentLineItems()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        const int producerRegistrationFeePence = 284200;
        const int lateRegistrationFeePence = 77200;
        const int producerClosedLoopRecyclingFeePence = 254800;
        const int subsidiariesFeePence = 647600;
        const int subsidiaryClrFeePence = 509600;
        const int subsidiaryClrCount = 2;
        const int totalFeePence = 1264800;
        const int outstandingPaymentPence = 1264800;
        const int netSubsidiaryFeePence = subsidiariesFeePence - subsidiaryClrFeePence;

        SetupFacadeMockRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .AsDirectLargeProducer()
                .WithProducerSubsidiaries(subsidiaryClrCount)
                .WithProducerClosedLoopRecycling(subsidiaryClrCount));

        SetupPaymentFacadeMockProducerRegistrationFee(
            ProducerPaymentResponseBuilder.Default()
                .WithProducerRegistrationFee(producerRegistrationFeePence)
                .WithProducerLateRegistrationFee(lateRegistrationFeePence)
                .WithProducerClosedLoopRecyclingFee(producerClosedLoopRecyclingFeePence)
                .WithTotalFee(totalFeePence)
                .WithOutstandingPayment(outstandingPaymentPence)
                .WithSubsidiariesFeeBreakdown(
                    subsidiariesFeePence,
                    subsidiaryClrFeePence,
                    subsidiaryClrCount));

        // Act
        var detailsPage = await GetAsPageModel<ManageRegistrationSubmissionDetailsPageModel>(
            $"/regulators/registration-submission-details/{submissionId}");

        // Assert
        using (new AssertionScope())
        {
            var subsidiaryCompanies = detailsPage.FindPaymentLineItem("Subsidiary companies");
            subsidiaryCompanies.Should().NotBeNull();
            subsidiaryCompanies!.Units.Should().Be(subsidiaryClrCount);
            subsidiaryCompanies.Amount.Should().Be(netSubsidiaryFeePence / 100m);

            var subsidiaryClr = detailsPage.FindPaymentLineItem("Subsidiaries closed loop packaging waste");
            subsidiaryClr.Should().NotBeNull();
            subsidiaryClr!.Units.Should().Be(subsidiaryClrCount);
            subsidiaryClr.Amount.Should().Be(subsidiaryClrFeePence / 100m);
        }
    }

    private void SetupPaymentFacadeMockComplianceSchemeRegistrationFee(CompliancePaymentResponseBuilder builder) =>
        FacadeServer.Given(Request.Create()
                .UsingPost()
                .WithPath("/compliance-scheme/registration-fee"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(builder.Build())));

    private void SetupPaymentFacadeMockProducerRegistrationFee(ProducerPaymentResponseBuilder? builder = null) =>
        FacadeServer.Given(Request.Create()
                .UsingPost()
                .WithPath("/producer/registration-fee"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize((builder ?? ProducerPaymentResponseBuilder.Default()).Build())));

    private void SetupFacadeMockRegistrationSubmissionDetails(RegistrationSubmissionDetailsBuilder builder) =>
        FacadeServer.Given(Request.Create()
                .UsingGet()
                .WithPath($"/api/organisation-registration-submission-details/{builder.SubmissionId}"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(builder.Build())));
}
