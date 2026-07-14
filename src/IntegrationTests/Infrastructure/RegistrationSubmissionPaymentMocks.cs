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
}
