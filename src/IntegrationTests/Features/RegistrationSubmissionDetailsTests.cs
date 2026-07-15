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

    [Fact]
    public async Task ShowsComplianceSchemePaymentDetailsWithPreviousPaymentOnResubmission()
    {
        // Arrange
        var submissionId = Guid.Parse("2c3d4e5f-6a7b-8c9d-0e1f-2a3b4c5d6e7f");
        var registrationBlobName = "c1d2e3f4-a5b6-7890-cdef-012345678901";

        SetupFacadeMockRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .WithOrganisationName("CSO Resubmitting Ltd")
                .WithOrganisationType("compliance")
                .WithIsResubmission(true, registrationBlobName));

        SetupPaymentFacadeMockComplianceSchemeRegistrationFeeByFileId(
            registrationBlobName,
            CompliancePaymentResponseBuilder.Default()
                .WithComplianceSchemeRegistrationFee(262000)
                .WithTotalFee(285400)
                .WithPreviousPayment(100000)
                .WithOutstandingPayment(185400));

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
            detailsPage.PreviousPaymentReceived.Should().Be(1000.00m);
            detailsPage.TotalOutstanding.Should().Be(1854.00m);
        }
    }

    [Fact]
    public async Task ShowsTotalOutstandingForProducerResubmissionAtKnownSubmissionId()
    {
        // Arrange
        var submissionId = Guid.Parse("c3d4e5f6-a7b8-9012-cdef-123456789013");
        var registrationBlobName = "d2e3f4a5-b6c7-8901-defa-123456789012";

        SetupFacadeMockRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .WithOrganisationName("Resubmission Direct Producer Ltd")
                .WithOrganisationType("large")
                .WithIsResubmission(true, registrationBlobName)
                .AsProducer("Large"));

        SetupPaymentFacadeMockProducerRegistrationFeeByFileId(
            registrationBlobName,
            ProducerPaymentResponseBuilder.Default()
                .WithProducerRegistrationFee(165800)
                .WithTotalFee(165800)
                .WithOutstandingPayment(82900)
                .WithOutstandingPayment(82900));

        // Act
        var detailsPage = await GetAsPageModel<ManageRegistrationSubmissionDetailsPageModel>(
            requestUri: $"/regulators/registration-submission-details/{submissionId}");

        // Assert
        using (new AssertionScope())
        {
            detailsPage.PaymentDetails.Should().NotBeNull();
            detailsPage.PaymentDetails!.HasPaymentSection.Should().BeTrue();
            detailsPage.TotalOutstanding.Should().Be(829.00m);
        }
    }

    [Fact]
    public async Task ShowsProducerPaymentDetailsWithPreviousPaymentOnResubmission()
    {
        // Arrange
        var submissionId = Guid.Parse("3d4e5f6a-7b8c-9d0e-1f2a-3b4c5d6e7f8a");
        var registrationBlobName = "d2e3f4a5-b6c7-8901-defa-123456789012";

        SetupFacadeMockRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .WithOrganisationName("Large Producer Ltd")
                .WithOrganisationType("large")
                .WithIsResubmission(true, registrationBlobName)
                .AsProducer("Large"));

        SetupPaymentFacadeMockProducerRegistrationFeeByFileId(
            registrationBlobName,
            ProducerPaymentResponseBuilder.Default()
                .WithProducerRegistrationFee(165800)
                .WithTotalFee(165800)
                .WithPreviousPayment(82900)
                .WithOutstandingPayment(82900));

        // Act
        var detailsPage = await GetAsPageModel<ManageRegistrationSubmissionDetailsPageModel>(
            requestUri: $"/regulators/registration-submission-details/{submissionId}");

        // Assert
        using (new AssertionScope())
        {
            detailsPage.PaymentDetails.Should().NotBeNull();
            detailsPage.PaymentDetails!.HasPaymentSection.Should().BeTrue();
            detailsPage.ApplicationFee.Should().Be(1658.00m);
            detailsPage.SubTotal.Should().Be(1658.00m);
            detailsPage.PreviousPaymentReceived.Should().Be(829.00m);
            detailsPage.TotalOutstanding.Should().Be(829.00m);
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

    private void SetupPaymentFacadeMockComplianceSchemeRegistrationFeeByFileId(string registrationBlobName, CompliancePaymentResponseBuilder builder) =>
        FacadeServer.Given(Request.Create()
                .UsingPost()
                .WithPath("/compliance-scheme/registration-fee")
                .WithBody(new WireMock.Matchers.JsonPartialMatcher(JsonSerializer.Serialize(new { RegistrationBlobName = registrationBlobName }), ignoreCase: true)))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(builder.Build())));

    private void SetupPaymentFacadeMockProducerRegistrationFeeByFileId(string registrationBlobName, ProducerPaymentResponseBuilder builder) =>
        FacadeServer.Given(Request.Create()
                .UsingPost()
                .WithPath("/producer/registration-fee")
                .WithBody(new WireMock.Matchers.JsonPartialMatcher(JsonSerializer.Serialize(new { RegistrationBlobName = registrationBlobName }), ignoreCase: true)))
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
