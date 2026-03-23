namespace IntegrationTests.Features;

using System.Text.Json;
using AwesomeAssertions;
using AwesomeAssertions.Execution;

using Builders;

using Infrastructure;
using PageModels;
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
    public async Task ShowsOrganisationDetailFromFacade()
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
    public async Task ShowsPaymentDetailsFromPaymentFacade()
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
    public async Task ShowsComplianceSchemePaymentDetailsWithPreviousPaymentOnResubmission()
    {
        // Arrange
        var submissionId = Guid.Parse("2c3d4e5f-6a7b-8c9d-0e1f-2a3b4c5d6e7f");
        var resubmissionFileId = "c1d2e3f4-a5b6-7890-cdef-012345678901";

        SetupFacadeMockRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .WithOrganisationName("CSO Resubmitting Ltd")
                .WithOrganisationType("compliance")
                .WithIsResubmission(true, resubmissionFileId));

        SetupPaymentFacadeMockComplianceSchemeRegistrationFeeByFileId(
            resubmissionFileId,
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
        var resubmissionFileId = "d2e3f4a5-b6c7-8901-defa-123456789012";

        SetupFacadeMockRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .WithOrganisationName("Resubmission Direct Producer Ltd")
                .WithOrganisationType("large")
                .WithIsResubmission(true, resubmissionFileId)
                .AsProducer("Large"));

        SetupPaymentFacadeMockProducerRegistrationFeeByFileId(
            resubmissionFileId,
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
            detailsPage.TotalOutstanding.Should().Be(829.00m);
        }
    }

    [Fact]
    public async Task ShowsProducerPaymentDetailsWithPreviousPaymentOnResubmission()
    {
        // Arrange
        var submissionId = Guid.Parse("3d4e5f6a-7b8c-9d0e-1f2a-3b4c5d6e7f8a");
        var resubmissionFileId = "d2e3f4a5-b6c7-8901-defa-123456789012";

        SetupFacadeMockRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .WithOrganisationName("Large Producer Ltd")
                .WithOrganisationType("large")
                .WithIsResubmission(true, resubmissionFileId)
                .AsProducer("Large"));

        SetupPaymentFacadeMockProducerRegistrationFeeByFileId(
            resubmissionFileId,
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

    private void SetupPaymentFacadeMockComplianceSchemeRegistrationFeeByFileId(string fileId, CompliancePaymentResponseBuilder builder) =>
        FacadeServer.Given(Request.Create()
                .UsingPost()
                .WithPath("/compliance-scheme/registration-fee")
                .WithBody(new WireMock.Matchers.JsonPartialMatcher(JsonSerializer.Serialize(new { FileId = fileId }), ignoreCase: true)))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(builder.Build())));

    private void SetupPaymentFacadeMockProducerRegistrationFeeByFileId(string fileId, ProducerPaymentResponseBuilder builder) =>
        FacadeServer.Given(Request.Create()
                .UsingPost()
                .WithPath("/producer/registration-fee")
                .WithBody(new WireMock.Matchers.JsonPartialMatcher(JsonSerializer.Serialize(new { FileId = fileId }), ignoreCase: true)))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(builder.Build())));

    private void SetupFacadeMockRegistrationSubmissionDetails(RegistrationSubmissionDetailsBuilder builder) =>
        FacadeServer.Given(Request.Create()
                .UsingGet()
                .WithPath($"/api/organisation-registration-submission-details/{builder.SubmissionId}"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(builder.Build())));
}
