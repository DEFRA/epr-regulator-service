namespace IntegrationTests.Infrastructure;

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
}
