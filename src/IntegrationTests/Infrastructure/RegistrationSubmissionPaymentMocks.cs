namespace IntegrationTests.Infrastructure;

using System.Text.Json;
using Builders;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

internal static class RegistrationSubmissionPaymentMocks
{
    internal static void SetupRegistrationSubmissionDetails(
        this WireMockServer facadeServer,
        RegistrationSubmissionDetailsBuilder builder) =>
        facadeServer.Given(Request.Create()
                .UsingGet()
                .WithPath($"/api/organisation-registration-submission-details/{builder.SubmissionId}"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(builder.Build())));

    internal static void SetupComplianceSchemeRegistrationFee(
        this WireMockServer facadeServer,
        CompliancePaymentResponseBuilder builder) =>
        facadeServer.Given(Request.Create()
                .UsingPost()
                .WithPath("/compliance-scheme/registration-fee"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(builder.Build())));

    internal static void SetupProducerRegistrationFee(
        this WireMockServer facadeServer,
        ProducerPaymentResponseBuilder? builder = null) =>
        facadeServer.Given(Request.Create()
                .UsingPost()
                .WithPath("/producer/registration-fee"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize((builder ?? ProducerPaymentResponseBuilder.Default()).Build())));

    /// <summary>
    /// by-submission GET, the snapshot-backed read path. Only matches requests carrying
    /// <c>requireSubmittedForApproval=true</c> - PaymentFacadeService.TryGetBySubmissionIdAsync
    /// always sends it, so a request missing it would indicate the wrong code path was hit.
    /// </summary>
    internal static void SetupProducerRegistrationFeeBySubmission(
        this WireMockServer facadeServer,
        Guid submissionId,
        ProducerPaymentResponseBuilder? builder = null) =>
        facadeServer.Given(Request.Create()
                .UsingGet()
                .WithPath($"/producer/registration-fee/{submissionId}")
                .WithParam("requireSubmittedForApproval", "true"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize((builder ?? ProducerPaymentResponseBuilder.Default()).Build())));

    internal static void SetupComplianceSchemeRegistrationFeeBySubmission(
        this WireMockServer facadeServer,
        Guid submissionId,
        CompliancePaymentResponseBuilder? builder = null) =>
        facadeServer.Given(Request.Create()
                .UsingGet()
                .WithPath($"/compliance-scheme/registration-fee/{submissionId}")
                .WithParam("requireSubmittedForApproval", "true"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize((builder ?? CompliancePaymentResponseBuilder.Default()).Build())));

    /// <summary>The "no snapshot yet" case - PaymentFacadeService.TryGetBySubmissionIdAsync
    /// treats 404 as "nothing to return", not an error, and falls back to the POST endpoint.</summary>
    internal static void SetupProducerRegistrationFeeBySubmissionNotFound(
        this WireMockServer facadeServer,
        Guid submissionId) =>
        facadeServer.Given(Request.Create()
                .UsingGet()
                .WithPath($"/producer/registration-fee/{submissionId}")
                .WithParam("requireSubmittedForApproval", "true"))
            .RespondWith(Response.Create().WithStatusCode(404));

    internal static void SetupComplianceSchemeRegistrationFeeBySubmissionNotFound(
        this WireMockServer facadeServer,
        Guid submissionId) =>
        facadeServer.Given(Request.Create()
                .UsingGet()
                .WithPath($"/compliance-scheme/registration-fee/{submissionId}")
                .WithParam("requireSubmittedForApproval", "true"))
            .RespondWith(Response.Create().WithStatusCode(404));

    /// <summary>An unexpected failure on the by-submission GET - exercises the
    /// catch-all fallback in PaymentFacadeService.TryGetBySubmissionIdAsync.</summary>
    internal static void SetupProducerRegistrationFeeBySubmissionServerError(
        this WireMockServer facadeServer,
        Guid submissionId) =>
        facadeServer.Given(Request.Create()
                .UsingGet()
                .WithPath($"/producer/registration-fee/{submissionId}")
                .WithParam("requireSubmittedForApproval", "true"))
            .RespondWith(Response.Create().WithStatusCode(500));

    internal static void SetupComplianceSchemeRegistrationFeeBySubmissionServerError(
        this WireMockServer facadeServer,
        Guid submissionId) =>
        facadeServer.Given(Request.Create()
                .UsingGet()
                .WithPath($"/compliance-scheme/registration-fee/{submissionId}")
                .WithParam("requireSubmittedForApproval", "true"))
            .RespondWith(Response.Create().WithStatusCode(500));
}
