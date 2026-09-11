namespace IntegrationTests.Infrastructure;

using AwesomeAssertions;
using WireMock.AwesomeAssertions;
using WireMock.Matchers;
using WireMock.Server;

internal static class PaymentFacadeRequestAssertions
{
    internal static void ShouldHavePostedProducerRegistrationFee(
        this WireMockServer facadeServer,
        object expectedPartialBody) =>
        facadeServer.Should().HaveReceivedACall()
            .UsingPost().And
            .AtPath("/producer/registration-fee").And
            .WithBodyAsJson(new JsonPartialMatcher(expectedPartialBody));

    internal static void ShouldHavePostedComplianceSchemeRegistrationFee(
        this WireMockServer facadeServer,
        object expectedPartialBody) =>
        facadeServer.Should().HaveReceivedACall()
            .UsingPost().And
            .AtPath("/compliance-scheme/registration-fee").And
            .WithBodyAsJson(new JsonPartialMatcher(expectedPartialBody));

    // The by-submission GET assertions below use WireMockServer.LogEntries (core, long-stable
    // API) rather than the AwesomeAssertions extension used for the two POST assertions above -
    // couldn't verify the extension's query-string/"not called" surface against this repo's
    // pinned package version in this environment (see the note in
    // FeeSnapshotBySubmissionTests.cs), so sticking to API already proven to work in this repo
    // (path/method matching, and the extension's plain "AtPath") rather than guessing further.
    // The requireSubmittedForApproval=true requirement is enforced at the stub match itself
    // (see RegistrationSubmissionPaymentMocks) - a request missing it gets no mapping and the
    // page fails a different way, so it's still covered indirectly.
    internal static void ShouldHaveGetProducerRegistrationFeeBySubmission(
        this WireMockServer facadeServer,
        Guid submissionId) =>
        facadeServer.LogEntries.Should().Contain(e =>
            e.RequestMessage.Method.Equals("GET", StringComparison.OrdinalIgnoreCase) &&
            e.RequestMessage.Path == $"/producer/registration-fee/{submissionId}");

    internal static void ShouldHaveGetComplianceSchemeRegistrationFeeBySubmission(
        this WireMockServer facadeServer,
        Guid submissionId) =>
        facadeServer.LogEntries.Should().Contain(e =>
            e.RequestMessage.Method.Equals("GET", StringComparison.OrdinalIgnoreCase) &&
            e.RequestMessage.Path == $"/compliance-scheme/registration-fee/{submissionId}");

    internal static void ShouldNotHavePostedProducerRegistrationFee(this WireMockServer facadeServer) =>
        facadeServer.LogEntries.Should().NotContain(e =>
            e.RequestMessage.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) &&
            e.RequestMessage.Path == "/producer/registration-fee");

    internal static void ShouldNotHavePostedComplianceSchemeRegistrationFee(this WireMockServer facadeServer) =>
        facadeServer.LogEntries.Should().NotContain(e =>
            e.RequestMessage.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) &&
            e.RequestMessage.Path == "/compliance-scheme/registration-fee");

    internal static void ShouldNotHaveGetProducerRegistrationFeeBySubmission(
        this WireMockServer facadeServer,
        Guid submissionId) =>
        facadeServer.LogEntries.Should().NotContain(e =>
            e.RequestMessage.Method.Equals("GET", StringComparison.OrdinalIgnoreCase) &&
            e.RequestMessage.Path == $"/producer/registration-fee/{submissionId}");

    internal static void ShouldNotHaveGetComplianceSchemeRegistrationFeeBySubmission(
        this WireMockServer facadeServer,
        Guid submissionId) =>
        facadeServer.LogEntries.Should().NotContain(e =>
            e.RequestMessage.Method.Equals("GET", StringComparison.OrdinalIgnoreCase) &&
            e.RequestMessage.Path == $"/compliance-scheme/registration-fee/{submissionId}");
}
