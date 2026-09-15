namespace IntegrationTests.Features;

using AwesomeAssertions;
using Builders;
using Infrastructure;
using PageModels;
using WireMock.AwesomeAssertions;
using WireMock.Matchers;

/// <summary>
/// Scenario 12 in its own class (rather than a method on <see cref="FeeSnapshotBySubmissionTests"/>)
/// because it needs a different <see cref="AdditionalConfiguration"/> - a distinct WireMockServer
/// and factory per class avoids state bleeding into the flag-on scenarios there.
/// </summary>
[Collection(SequentialCollection.Sequential)]
public class FeeSnapshotBySubmissionFeatureFlagOffTests : IntegrationTestBase
{
    protected override IDictionary<string, string?>? AdditionalConfiguration => new Dictionary<string, string?>
    {
        ["FeatureManagement:EnableRegistrationFeeCalculationViaPaymentService"] = "false",
    };

    public override Task InitializeAsync()
    {
        base.InitializeAsync();
        SetupUserAccountsMock();
        return Task.CompletedTask;
    }

    [Fact]
    [Trait("Scenario", "12")]
    public async Task FlagOff_NeverCallsBySubmissionGet_UsesLegacyPostOnly()
    {
        var submissionId = FeeSnapshotScenarioIds.FeatureFlagOff;
        var appRef = FeeSnapshotScenarioIds.AppRef("12");

        FacadeServer.SetupRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .AsDirectLargeProducer()
                .WithApplicationReferenceNumber(appRef));

        FacadeServer.SetupProducerRegistrationFee(
            ProducerPaymentResponseBuilder.Default()
                .WithProducerRegistrationFee(284200)
                .WithTotalFee(284200)
                .WithOutstandingPayment(284200));

        var page = await GetAsPageModel<ManageRegistrationSubmissionDetailsPageModel>(
            $"/regulators/registration-submission-details/{submissionId}");

        page.PaymentDetails!.HasPaymentSection.Should().BeTrue();

        FacadeServer.LogEntries.Should().NotContain(e =>
            e.RequestMessage.Method.Equals("GET", StringComparison.OrdinalIgnoreCase) &&
            e.RequestMessage.Path == $"/producer/registration-fee/{submissionId}");
        FacadeServer.Should().HaveReceivedACall()
            .UsingPost().And
            .AtPath("/producer/registration-fee").And
            .WithBodyAsJson(new JsonPartialMatcher(new { applicationReferenceNumber = appRef }));
    }
}
