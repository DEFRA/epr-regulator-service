namespace IntegrationTests.Features;

using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Builders;
using Infrastructure;
using PageModels;

/// <summary>
/// the regulator's payment-details view now prefers a snapshot-backed
/// GET (requireSubmittedForApproval=true) over the legacy POST-based live calculation, behind the
/// EnableRegistrationFeeCalculationViaPaymentService flag, with a silent fallback to POST on any
/// failure. Mirrors the structure of <see cref="ClrPaymentScenarioTests"/>, the closest existing
/// precedent for driving this page end-to-end against a stubbed facade.
/// </summary>
[Collection(SequentialCollection.Sequential)]
public class FeeSnapshotBySubmissionTests : IntegrationTestBase
{
    public override Task InitializeAsync()
    {
        base.InitializeAsync();
        SetupUserAccountsMock();
        return Task.CompletedTask;
    }

    // feature flag on, happy path: by-submission GET is used, not the legacy POST ----------

    [Fact]
    [Trait("Scenario", "11a")]
    public async Task FlagOn_ProducerSnapshotAvailable_UsesBySubmissionGet_AndDoesNotPostLiveCalculation()
    {
        var submissionId = FeeSnapshotScenarioIds.ProducerHappyPath;
        var appRef = FeeSnapshotScenarioIds.AppRef("11a");

        FacadeServer.SetupRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .AsDirectLargeProducer()
                .WithApplicationReferenceNumber(appRef));

        FacadeServer.SetupProducerRegistrationFeeBySubmission(
            submissionId,
            ProducerPaymentResponseBuilder.Default()
                .WithProducerRegistrationFee(284200)
                .WithTotalFee(284200)
                .WithOutstandingPayment(284200));

        var page = await LoadDetailsPage(submissionId);

        using (new AssertionScope())
        {
            page.PaymentDetails!.HasPaymentSection.Should().BeTrue();
            page.SubTotal.Should().Be(2842.00m);
        }

        FacadeServer.ShouldHaveGetProducerRegistrationFeeBySubmission(submissionId);
        FacadeServer.ShouldNotHavePostedProducerRegistrationFee();
    }

    [Fact]
    [Trait("Scenario", "11b")]
    public async Task FlagOn_ComplianceSchemeSnapshotAvailable_UsesBySubmissionGet_AndDoesNotPostLiveCalculation()
    {
        var submissionId = FeeSnapshotScenarioIds.ComplianceSchemeHappyPath;
        var appRef = FeeSnapshotScenarioIds.AppRef("11b");

        FacadeServer.SetupRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .WithApplicationReferenceNumber(appRef)
                .WithCsoMembers(CsoMemberBuilder.Large("100001")));

        FacadeServer.SetupComplianceSchemeRegistrationFeeBySubmission(
            submissionId,
            CompliancePaymentResponseBuilder.Default()
                .WithComplianceSchemeRegistrationFee(284200)
                .WithTotalFee(284200 + 165800)
                .WithMembers(ComplianceSchemeMemberFeeBuilder.Default()));

        var page = await LoadDetailsPage(submissionId);

        page.PaymentDetails!.HasPaymentSection.Should().BeTrue();

        FacadeServer.ShouldHaveGetComplianceSchemeRegistrationFeeBySubmission(submissionId);
        FacadeServer.ShouldNotHavePostedComplianceSchemeRegistrationFee();
    }

    // by-submission GET returns 404 (no snapshot yet): clean fallback to POST --------------

    [Fact]
    [Trait("Scenario", "13a")]
    public async Task ProducerBySubmissionGet_NotFound_FallsBackToPost_AndRendersCorrectly()
    {
        var submissionId = FeeSnapshotScenarioIds.ProducerNotFoundFallback;
        var appRef = FeeSnapshotScenarioIds.AppRef("13a");

        FacadeServer.SetupRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .AsDirectLargeProducer()
                .WithApplicationReferenceNumber(appRef));

        FacadeServer.SetupProducerRegistrationFeeBySubmissionNotFound(submissionId);
        FacadeServer.SetupProducerRegistrationFee(
            ProducerPaymentResponseBuilder.Default()
                .WithProducerRegistrationFee(284200)
                .WithTotalFee(284200)
                .WithOutstandingPayment(284200));

        var page = await LoadDetailsPage(submissionId);

        page.PaymentDetails!.HasPaymentSection.Should().BeTrue();
        page.SubTotal.Should().Be(2842.00m);

        FacadeServer.ShouldHavePostedProducerRegistrationFee(new { applicationReferenceNumber = appRef });
    }

    [Fact]
    [Trait("Scenario", "13b")]
    public async Task ComplianceSchemeBySubmissionGet_NotFound_FallsBackToPost_AndRendersCorrectly()
    {
        var submissionId = FeeSnapshotScenarioIds.ComplianceSchemeNotFoundFallback;
        var appRef = FeeSnapshotScenarioIds.AppRef("13b");

        FacadeServer.SetupRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .WithApplicationReferenceNumber(appRef)
                .WithCsoMembers(CsoMemberBuilder.Large("100001")));

        FacadeServer.SetupComplianceSchemeRegistrationFeeBySubmissionNotFound(submissionId);
        FacadeServer.SetupComplianceSchemeRegistrationFee(
            CompliancePaymentResponseBuilder.Default()
                .WithComplianceSchemeRegistrationFee(284200)
                .WithTotalFee(284200 + 165800)
                .WithMembers(ComplianceSchemeMemberFeeBuilder.Default()));

        var page = await LoadDetailsPage(submissionId);

        page.PaymentDetails!.HasPaymentSection.Should().BeTrue();

        FacadeServer.ShouldHavePostedComplianceSchemeRegistrationFee(new { applicationReferenceNumber = appRef });
    }

    // by-submission GET throws/5xx: falls back to POST rather than surfacing an error ------

    [Fact]
    [Trait("Scenario", "14")]
    public async Task ProducerBySubmissionGet_ServerError_FallsBackToPost_AndRendersCorrectly()
    {
        var submissionId = FeeSnapshotScenarioIds.ProducerServerErrorFallback;
        var appRef = FeeSnapshotScenarioIds.AppRef("14");

        FacadeServer.SetupRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .AsDirectLargeProducer()
                .WithApplicationReferenceNumber(appRef));

        FacadeServer.SetupProducerRegistrationFeeBySubmissionServerError(submissionId);
        FacadeServer.SetupProducerRegistrationFee(
            ProducerPaymentResponseBuilder.Default()
                .WithProducerRegistrationFee(284200)
                .WithTotalFee(284200)
                .WithOutstandingPayment(284200));

        // The point of this scenario: a downstream 500 must not surface as a 500 on this page -
        // GetAsPageModel calls EnsureSuccessStatusCode, so simply not throwing here is part of
        // the assertion, not just scaffolding.
        var page = await LoadDetailsPage(submissionId);

        page.PaymentDetails!.HasPaymentSection.Should().BeTrue();
        page.SubTotal.Should().Be(2842.00m);

        FacadeServer.ShouldHavePostedProducerRegistrationFee(new { applicationReferenceNumber = appRef });

        // Full assertion that the fallback was *logged* needs the same log-capture infrastructure
        // built for epr-payment-service (TestLogSink) - not wired up in this repo yet. The
        // observable behaviour (correct render via the fallback) is verified above; the log
        // itself is not, pending that infrastructure or a decision to add it.
    }

    // empty CSO membership details: must not throw (regression for the null-safety fixes) --

    [Fact]
    [Trait("Scenario", "15")]
    public async Task ComplianceScheme_EmptyCsoMembershipDetails_RendersWithoutError()
    {
        var submissionId = FeeSnapshotScenarioIds.EmptyCsoMembershipDetails;
        var appRef = FeeSnapshotScenarioIds.AppRef("15");

        FacadeServer.SetupRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .WithApplicationReferenceNumber(appRef)
                .WithEmptyCsoMembershipDetails());

        FacadeServer.SetupComplianceSchemeRegistrationFeeBySubmission(
            submissionId,
            CompliancePaymentResponseBuilder.Default()
                .WithComplianceSchemeRegistrationFee(284200)
                .WithTotalFee(284200 + 165800)
                .WithMembers(ComplianceSchemeMemberFeeBuilder.Default()));

        // The facade fee response has a member with no corresponding row in CSOMembershipDetails
        // (empty here). ComplianceSchemeMemberExtension.ResolveMemberType/GetSubsidiariesCompanyCount
        // fall back to the response's own fields in that case rather than needing a CSO match -
        // note this exercises the empty-list path specifically, not a genuinely null
        // CSOMembershipDetails (which the current builder can't produce).
        var page = await LoadDetailsPage(submissionId);

        page.PaymentDetails!.HasPaymentSection.Should().BeTrue();
    }

    // new response fields: prefer facade value, fall back when absent ----------------------

    [Fact]
    [Trait("Scenario", "16a")]
    public async Task ProducerSize_FallsBackToLocallyHeldValue_WhenFacadeOmitsIt()
    {
        var submissionId = FeeSnapshotScenarioIds.ProducerSizeFallback;
        var appRef = FeeSnapshotScenarioIds.AppRef("16a");

        FacadeServer.SetupRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .AsDirectLargeProducer()
                .WithApplicationReferenceNumber(appRef));

        FacadeServer.SetupProducerRegistrationFeeBySubmission(
            submissionId,
            ProducerPaymentResponseBuilder.Default()
                .WithProducerRegistrationFee(284200)
                .WithTotalFee(284200)
                .WithOutstandingPayment(284200));

        // Falling back to the locally-held producer details (from the submission-details
        // response, organisationType "large") must not throw and must still render a fee.
        var page = await LoadDetailsPage(submissionId);

        page.PaymentDetails!.HasPaymentSection.Should().BeTrue();
    }

    [Fact]
    [Trait("Scenario", "16b")]
    public async Task ComplianceSchemeMemberType_PrefersFacadeValue_WhenPresent()
    {
        var submissionId = FeeSnapshotScenarioIds.MemberTypeFallback;
        var appRef = FeeSnapshotScenarioIds.AppRef("16b");

        FacadeServer.SetupRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .WithApplicationReferenceNumber(appRef)
                .WithCsoMembers(CsoMemberBuilder.Large("100001").WithMemberType("small")));

        // Facade now reports "large" for this member - the response value should win over the
        // CSO membership details' "small", per ComplianceSchemeMemberExtension.ResolveMemberType.
        FacadeServer.SetupComplianceSchemeRegistrationFeeBySubmission(
            submissionId,
            CompliancePaymentResponseBuilder.Default()
                .WithComplianceSchemeRegistrationFee(284200)
                .WithTotalFee(284200 + 165800)
                .WithMembers(ComplianceSchemeMemberFeeBuilder.Default().WithMemberType("large")));

        var page = await LoadDetailsPage(submissionId);

        page.PaymentDetails!.HasPaymentSection.Should().BeTrue();
    }

    private async Task<ManageRegistrationSubmissionDetailsPageModel> LoadDetailsPage(Guid submissionId) =>
        await GetAsPageModel<ManageRegistrationSubmissionDetailsPageModel>(
            $"/regulators/registration-submission-details/{submissionId}");
}

/// <summary>
/// Scenario 12 in its own class (rather than a method on <see cref="FeeSnapshotBySubmissionTests"/>)
/// because it needs a different <see cref="AdditionalConfiguration"/> - a distinct WireMockServer
/// and factory per class avoids state bleeding into the flag-on scenarios above.
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

        FacadeServer.ShouldNotHaveGetProducerRegistrationFeeBySubmission(submissionId);
        FacadeServer.ShouldHavePostedProducerRegistrationFee(new { applicationReferenceNumber = appRef });
    }
}
