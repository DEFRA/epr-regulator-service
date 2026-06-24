namespace IntegrationTests.Features;

using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Builders;
using Infrastructure;
using PageModels;

[Collection(SequentialCollection.Sequential)]
public class ClrPaymentScenarioTests : IntegrationTestBase
{
    public override Task InitializeAsync()
    {
        base.InitializeAsync();
        SetupUserAccountsMock();
        return Task.CompletedTask;
    }

    [Fact]
    [Trait("Scenario", "01")]
    public async Task DirectProducer_NoSubsidiaries_HcClrOff_SubClrOff_DisplaysNoClrLines_AndPostsProducerFeeRequest()
    {
        const int scenario = 1;
        var submissionId = ClrPaymentScenarioIds.Scenario01;
        var appRef = ClrPaymentScenarioIds.AppRef(scenario);

        FacadeServer.SetupRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .AsDirectLargeProducer()
                .WithApplicationReferenceNumber(appRef));

        FacadeServer.SetupProducerRegistrationFee(
            ProducerPaymentResponseBuilder.Default()
                .WithProducerRegistrationFee(284200)
                .WithProducerLateRegistrationFee(77200)
                .WithTotalFee(361400)
                .WithOutstandingPayment(361400));

        var page = await LoadDetailsPage(submissionId);

        using (new AssertionScope())
        {
            page.PaymentDetails!.HasPaymentSection.Should().BeTrue();
            page.ApplicationFee.Should().Be(2842.00m);
            page.SubTotal.Should().Be(3614.00m);
            page.TotalOutstanding.Should().Be(3614.00m);
            AssertDirectProducerHasNoClrLines(page);
        }

        FacadeServer.ShouldHavePostedProducerRegistrationFee(new
        {
            applicationReferenceNumber = appRef,
            producerType = "large",
            isClosedLoopRecycling = false,
            noOfHoldingCompaniesClosedLoopRecycling = 0,
            noOfSubsidiariesClosedLoopRecycling = 0,
        });
    }

    [Fact]
    [Trait("Scenario", "02")]
    public async Task DirectProducer_NoSubsidiaries_HcClrOn_SubClrOff_DisplaysHcClrLine_AndPostsProducerFeeRequest()
    {
        const int scenario = 2;
        var submissionId = ClrPaymentScenarioIds.Scenario02;
        var appRef = ClrPaymentScenarioIds.AppRef(scenario);
        const int hcClrFeePence = 254800;

        FacadeServer.SetupRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .AsDirectLargeProducer()
                .WithApplicationReferenceNumber(appRef)
                .WithProducerClosedLoopRecycling(0, 1));

        FacadeServer.SetupProducerRegistrationFee(
            ProducerPaymentResponseBuilder.Default()
                .WithProducerRegistrationFee(284200)
                .WithProducerLateRegistrationFee(77200)
                .WithProducerClosedLoopRecyclingFee(hcClrFeePence)
                .WithTotalFee(616200)
                .WithOutstandingPayment(616200));

        var page = await LoadDetailsPage(submissionId);

        using (new AssertionScope())
        {
            page.SubTotal.Should().Be(6162.00m);
            page.TotalOutstanding.Should().Be(6162.00m);
            AssertDirectProducerHcClrLine(page, units: 1, amountPence: hcClrFeePence);
            page.FindSubsidiaryClrLine().Should().BeNull();
        }

        FacadeServer.ShouldHavePostedProducerRegistrationFee(new
        {
            applicationReferenceNumber = appRef,
            producerType = "large",
            isClosedLoopRecycling = true,
            noOfHoldingCompaniesClosedLoopRecycling = 1,
            noOfSubsidiariesClosedLoopRecycling = 0,
        });
    }

    [Fact]
    [Trait("Scenario", "03")]
    public async Task DirectProducer_3Subsidiaries_HcClrOff_SubClrOff_DisplaysSubsidiaryLineOnly_AndPostsProducerFeeRequest()
    {
        const int scenario = 3;
        var submissionId = ClrPaymentScenarioIds.Scenario03;
        var appRef = ClrPaymentScenarioIds.AppRef(scenario);
        const int subsidiariesFeePence = 138000;

        FacadeServer.SetupRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .AsDirectLargeProducer()
                .WithApplicationReferenceNumber(appRef)
                .WithProducerSubsidiaries(3));

        FacadeServer.SetupProducerRegistrationFee(
            ProducerPaymentResponseBuilder.Default()
                .WithProducerRegistrationFee(284200)
                .WithProducerLateRegistrationFee(77200)
                .WithTotalFee(499400)
                .WithOutstandingPayment(499400)
                .WithSubsidiariesFeeBreakdown(subsidiariesFeePence, 0, 0));

        var page = await LoadDetailsPage(submissionId);

        using (new AssertionScope())
        {
            page.SubTotal.Should().Be(4994.00m);
            AssertDirectProducerSubsidiaryCompaniesLine(page, units: 3, amountPence: subsidiariesFeePence);
            AssertDirectProducerHasNoClrLines(page);
        }

        FacadeServer.ShouldHavePostedProducerRegistrationFee(new
        {
            applicationReferenceNumber = appRef,
            isClosedLoopRecycling = false,
            noOfSubsidiariesClosedLoopRecycling = 0,
        });
    }

    [Fact]
    [Trait("Scenario", "04")]
    public async Task DirectProducer_3Subsidiaries_HcClrOn_SubClr3_DisplaysAllClrLines_AndPostsProducerFeeRequest()
    {
        const int scenario = 4;
        var submissionId = ClrPaymentScenarioIds.Scenario04;
        var appRef = ClrPaymentScenarioIds.AppRef(scenario);
        const int hcClrFeePence = 254800;
        const int subsidiariesFeePence = 647600;
        const int subsidiaryClrFeePence = 509600;
        const int netSubsidiaryFeePence = subsidiariesFeePence - subsidiaryClrFeePence;

        FacadeServer.SetupRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .AsDirectLargeProducer()
                .WithApplicationReferenceNumber(appRef)
                .WithProducerSubsidiaries(3)
                .WithProducerClosedLoopRecycling(3, 1));

        FacadeServer.SetupProducerRegistrationFee(
            ProducerPaymentResponseBuilder.Default()
                .WithProducerRegistrationFee(284200)
                .WithProducerLateRegistrationFee(77200)
                .WithProducerClosedLoopRecyclingFee(hcClrFeePence)
                .WithTotalFee(1773800)
                .WithOutstandingPayment(1773800)
                .WithSubsidiariesFeeBreakdown(subsidiariesFeePence, subsidiaryClrFeePence, 3));

        var page = await LoadDetailsPage(submissionId);

        using (new AssertionScope())
        {
            page.SubTotal.Should().Be(17738.00m);
            AssertDirectProducerHcClrLine(page, units: 1, amountPence: hcClrFeePence);
            AssertDirectProducerSubsidiaryCompaniesLine(page, units: 3, amountPence: netSubsidiaryFeePence);
            AssertDirectProducerSubsidiaryClrLine(page, units: 3, amountPence: subsidiaryClrFeePence);
        }

        FacadeServer.ShouldHavePostedProducerRegistrationFee(new
        {
            applicationReferenceNumber = appRef,
            isClosedLoopRecycling = true,
            noOfHoldingCompaniesClosedLoopRecycling = 1,
            noOfSubsidiariesClosedLoopRecycling = 3,
        });
    }

    [Fact]
    [Trait("Scenario", "05")]
    public async Task Cso_1Member_HcClrOff_SubClrOff_DisplaysNoClrLines_AndPostsComplianceFeeRequest()
    {
        const int scenario = 5;
        var submissionId = ClrPaymentScenarioIds.Scenario05;
        var appRef = ClrPaymentScenarioIds.AppRef(scenario);

        FacadeServer.SetupRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .WithOrganisationType("compliance")
                .WithRegistrationJourneyType("CsoLargeProducer")
                .WithApplicationReferenceNumber(appRef));

        FacadeServer.SetupComplianceSchemeRegistrationFee(
            CompliancePaymentResponseBuilder.Default()
                .WithComplianceSchemeRegistrationFee(284200)
                .WithTotalFee(566300)
                .WithMembers(ComplianceSchemeMemberFeeBuilder.Default()
                    .WithMemberId("100001")
                    .WithLateRegistrationFee(77200)
                    .WithTotalMemberFee(566300)));

        var page = await LoadDetailsPage(submissionId);

        using (new AssertionScope())
        {
            page.ApplicationFee.Should().Be(2842.00m);
            page.SubTotal.Should().Be(5663.00m);
            AssertCsoHasNoClrLines(page);
        }

        FacadeServer.ShouldHavePostedComplianceSchemeRegistrationFee(new
        {
            applicationReferenceNumber = appRef,
            complianceSchemeMembers = new[]
            {
                new
                {
                    memberId = "100001",
                    memberType = "large",
                    isClosedLoopRecycling = false,
                    noOfHoldingCompaniesClosedLoopRecycling = 0,
                    noOfSubsidiariesClosedLoopRecycling = 0,
                },
            },
        });
    }

    [Fact]
    [Trait("Scenario", "06")]
    public async Task Cso_1Member_HcClrOn_SubClrOff_DisplaysHcClrLine_AndZerosSubClrInRequest()
    {
        const int scenario = 6;
        var submissionId = ClrPaymentScenarioIds.Scenario06;
        var appRef = ClrPaymentScenarioIds.AppRef(scenario);
        const int hcClrFeePence = 254800;

        FacadeServer.SetupRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .WithOrganisationType("compliance")
                .WithRegistrationJourneyType("CsoLargeProducer")
                .WithApplicationReferenceNumber(appRef)
                .WithCsoMemberSubsidiaries(1)
                .WithCsoMemberClosedLoopRecycling(8));

        FacadeServer.SetupComplianceSchemeRegistrationFee(
            CompliancePaymentResponseBuilder.Default()
                .WithComplianceSchemeRegistrationFee(284200)
                .WithTotalFee(821100)
                .WithMembers(ComplianceSchemeMemberFeeBuilder.Default()
                    .WithMemberId("100001")
                    .WithLateRegistrationFee(77200)
                    .WithMemberClosedLoopRecyclingFee(hcClrFeePence)
                    .WithTotalMemberFee(821100)));

        var page = await LoadDetailsPage(submissionId);

        using (new AssertionScope())
        {
            page.SubTotal.Should().Be(8211.00m);
            AssertCsoHcClrLine(page, units: 1, amountPence: hcClrFeePence);
            page.FindSubsidiaryClrLine().Should().BeNull();
        }

        FacadeServer.ShouldHavePostedComplianceSchemeRegistrationFee(new
        {
            applicationReferenceNumber = appRef,
            complianceSchemeMembers = new[]
            {
                new
                {
                    memberId = "100001",
                    memberType = "large",
                    isClosedLoopRecycling = true,
                    noOfHoldingCompaniesClosedLoopRecycling = 1,
                    noOfSubsidiariesClosedLoopRecycling = 0,
                },
            },
        });
    }

    [Fact]
    [Trait("Scenario", "07")]
    public async Task Cso_3Members_3SubsEach_HcClrOff_SubClrOff_DisplaysAggregatedSubsidiaries_AndPostsComplianceFeeRequest()
    {
        const int scenario = 7;
        var submissionId = ClrPaymentScenarioIds.Scenario07;
        var appRef = ClrPaymentScenarioIds.AppRef(scenario);
        const int subsidiariesFeePerMemberPence = 138000;
        const int memberRegistrationFeePence = 165800;
        const int totalFeePence = 284200 + (memberRegistrationFeePence + subsidiariesFeePerMemberPence) * 3;

        FacadeServer.SetupRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .WithApplicationReferenceNumber(appRef)
                .WithCsoMembers(
                    CsoMemberBuilder.Large("100001").WithSubsidiaries(3),
                    CsoMemberBuilder.Large("100002").WithSubsidiaries(3),
                    CsoMemberBuilder.Large("100003").WithSubsidiaries(3)));

        FacadeServer.SetupComplianceSchemeRegistrationFee(
            CompliancePaymentResponseBuilder.Default()
                .WithComplianceSchemeRegistrationFee(284200)
                .WithTotalFee(totalFeePence)
                .WithMembers(
                    BuildCsoMemberWithSubsidiaries("100001", memberRegistrationFeePence, subsidiariesFeePerMemberPence),
                    BuildCsoMemberWithSubsidiaries("100002", memberRegistrationFeePence, subsidiariesFeePerMemberPence),
                    BuildCsoMemberWithSubsidiaries("100003", memberRegistrationFeePence, subsidiariesFeePerMemberPence)));

        var page = await LoadDetailsPage(submissionId);

        using (new AssertionScope())
        {
            page.SubTotal.Should().Be(totalFeePence / 100m);
            AssertCsoLargeProducersLine(page, units: 3);
            AssertCsoSubsidiaryCompaniesLine(page, units: 9, amountPence: subsidiariesFeePerMemberPence * 3);
            AssertCsoHasNoClrLines(page);
        }

        FacadeServer.ShouldHavePostedComplianceSchemeRegistrationFee(new
        {
            applicationReferenceNumber = appRef,
            complianceSchemeMembers = new[]
            {
                new { memberId = "100001", isClosedLoopRecycling = false, noOfSubsidiariesClosedLoopRecycling = 0 },
                new { memberId = "100002", isClosedLoopRecycling = false, noOfSubsidiariesClosedLoopRecycling = 0 },
                new { memberId = "100003", isClosedLoopRecycling = false, noOfSubsidiariesClosedLoopRecycling = 0 },
            },
        });
    }

    [Fact]
    [Trait("Scenario", "08")]
    [Trait("Category", "AMCR-291")]
    public async Task Cso_3Members_3SubsEach_HcClr3_SubClr3_DisplaysAggregatedClrFees_AndZerosSubClrInRequest()
    {
        const int scenario = 8;
        var submissionId = ClrPaymentScenarioIds.Scenario08;
        var appRef = ClrPaymentScenarioIds.AppRef(scenario);
        const int hcClrFeePence = 254800;
        const int subsidiariesFeePence = 647600;
        const int subsidiaryClrFeePence = 509600;
        const int netSubsidiaryFeePence = subsidiariesFeePence - subsidiaryClrFeePence;

        FacadeServer.SetupRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .WithApplicationReferenceNumber(appRef)
                .WithCsoMembers(
                    CsoMemberBuilder.Large("100001")
                        .WithHoldingCompanyClrCount(3)
                        .WithSubsidiaries(3)
                        .WithSubsidiariesClosedLoopRecycling(3),
                    CsoMemberBuilder.Large("100002")
                        .WithSubsidiaries(3)
                        .WithSubsidiariesClosedLoopRecycling(3),
                    CsoMemberBuilder.Large("100003")
                        .WithSubsidiaries(3)
                        .WithSubsidiariesClosedLoopRecycling(3)));

        FacadeServer.SetupComplianceSchemeRegistrationFee(
            CompliancePaymentResponseBuilder.Default()
                .WithComplianceSchemeRegistrationFee(284200)
                .WithTotalFee(5000000)
                .WithMembers(
                    BuildCsoMemberWithSubsidiaryClr("100001", hcClrFeePence, subsidiariesFeePence, subsidiaryClrFeePence, 3),
                    BuildCsoMemberWithSubsidiaryClr("100002", hcClrFeePence, subsidiariesFeePence, subsidiaryClrFeePence, 3),
                    BuildCsoMemberWithSubsidiaryClr("100003", hcClrFeePence, subsidiariesFeePence, subsidiaryClrFeePence, 3)));

        var page = await LoadDetailsPage(submissionId);

        using (new AssertionScope())
        {
            AssertCsoHcClrLine(page, units: 3, amountPence: hcClrFeePence * 3);
            AssertCsoSubsidiaryCompaniesLine(page, units: 9, amountPence: netSubsidiaryFeePence * 3);
            AssertCsoSubsidiaryClrLine(page, units: 9, amountPence: subsidiaryClrFeePence * 3);
        }

        FacadeServer.ShouldHavePostedComplianceSchemeRegistrationFee(new
        {
            applicationReferenceNumber = appRef,
            complianceSchemeMembers = new[]
            {
                new { memberId = "100001", isClosedLoopRecycling = true, noOfHoldingCompaniesClosedLoopRecycling = 1, noOfSubsidiariesClosedLoopRecycling = 0 },
                new { memberId = "100002", isClosedLoopRecycling = true, noOfHoldingCompaniesClosedLoopRecycling = 1, noOfSubsidiariesClosedLoopRecycling = 0 },
                new { memberId = "100003", isClosedLoopRecycling = true, noOfHoldingCompaniesClosedLoopRecycling = 1, noOfSubsidiariesClosedLoopRecycling = 0 },
            },
        });
    }

    [Fact]
    [Trait("Scenario", "09")]
    public async Task DirectProducer_3Subsidiaries_HcClrOn_SubClrOff_DisplaysHcClrAndFullSubsidiaryFee_AndPostsProducerFeeRequest()
    {
        const int scenario = 9;
        var submissionId = ClrPaymentScenarioIds.Scenario09;
        var appRef = ClrPaymentScenarioIds.AppRef(scenario);
        const int hcClrFeePence = 254800;
        const int subsidiariesFeePence = 647600;

        FacadeServer.SetupRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .AsDirectLargeProducer()
                .WithApplicationReferenceNumber(appRef)
                .WithProducerSubsidiaries(3)
                .WithProducerClosedLoopRecycling(0, 1));

        FacadeServer.SetupProducerRegistrationFee(
            ProducerPaymentResponseBuilder.Default()
                .WithProducerRegistrationFee(284200)
                .WithProducerLateRegistrationFee(77200)
                .WithProducerClosedLoopRecyclingFee(hcClrFeePence)
                .WithTotalFee(1263800)
                .WithOutstandingPayment(1263800)
                .WithSubsidiariesFeeBreakdown(subsidiariesFeePence, 0, 0));

        var page = await LoadDetailsPage(submissionId);

        using (new AssertionScope())
        {
            page.SubTotal.Should().Be(12638.00m);
            AssertDirectProducerHcClrLine(page, units: 1, amountPence: hcClrFeePence);
            AssertDirectProducerSubsidiaryCompaniesLine(page, units: 3, amountPence: subsidiariesFeePence);
            page.FindSubsidiaryClrLine().Should().BeNull();
        }

        FacadeServer.ShouldHavePostedProducerRegistrationFee(new
        {
            applicationReferenceNumber = appRef,
            isClosedLoopRecycling = true,
            noOfHoldingCompaniesClosedLoopRecycling = 1,
            noOfSubsidiariesClosedLoopRecycling = 0,
        });
    }

    [Fact]
    [Trait("Scenario", "10")]
    public async Task DirectProducer_3Subsidiaries_HcClrOff_SubClr2_DisplaysPartialSubClrOnly_AndPostsProducerFeeRequest()
    {
        const int scenario = 10;
        var submissionId = ClrPaymentScenarioIds.Scenario10;
        var appRef = ClrPaymentScenarioIds.AppRef(scenario);
        const int subsidiariesFeePence = 647600;
        const int subsidiaryClrFeePence = 339700;
        const int netSubsidiaryFeePence = subsidiariesFeePence - subsidiaryClrFeePence;

        FacadeServer.SetupRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .AsDirectLargeProducer()
                .WithApplicationReferenceNumber(appRef)
                .WithProducerSubsidiaries(3)
                .WithProducerClosedLoopRecycling(2, 0));

        FacadeServer.SetupProducerRegistrationFee(
            ProducerPaymentResponseBuilder.Default()
                .WithProducerRegistrationFee(284200)
                .WithProducerLateRegistrationFee(77200)
                .WithTotalFee(1271000)
                .WithOutstandingPayment(1271000)
                .WithSubsidiariesFeeBreakdown(subsidiariesFeePence, subsidiaryClrFeePence, 2));

        var page = await LoadDetailsPage(submissionId);

        using (new AssertionScope())
        {
            page.SubTotal.Should().Be(12710.00m);
            page.FindDirectProducerHoldingCompanyClrLine().Should().BeNull();
            AssertDirectProducerSubsidiaryCompaniesLine(page, units: 3, amountPence: netSubsidiaryFeePence);
            AssertDirectProducerSubsidiaryClrLine(page, units: 2, amountPence: subsidiaryClrFeePence);
        }

        FacadeServer.ShouldHavePostedProducerRegistrationFee(new
        {
            applicationReferenceNumber = appRef,
            isClosedLoopRecycling = false,
            noOfHoldingCompaniesClosedLoopRecycling = 0,
            noOfSubsidiariesClosedLoopRecycling = 2,
        });
    }

    [Fact]
    [Trait("Scenario", "11")]
    public async Task DirectProducer_3Subsidiaries_HcClrOn_SubClr2_DisplaysBothClrTypes_AndPostsProducerFeeRequest()
    {
        const int scenario = 11;
        var submissionId = ClrPaymentScenarioIds.Scenario11;
        var appRef = ClrPaymentScenarioIds.AppRef(scenario);
        const int hcClrFeePence = 254800;
        const int subsidiariesFeePence = 647600;
        const int subsidiaryClrFeePence = 339700;
        const int netSubsidiaryFeePence = subsidiariesFeePence - subsidiaryClrFeePence;

        FacadeServer.SetupRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .AsDirectLargeProducer()
                .WithApplicationReferenceNumber(appRef)
                .WithProducerSubsidiaries(3)
                .WithProducerClosedLoopRecycling(2, 1));

        FacadeServer.SetupProducerRegistrationFee(
            ProducerPaymentResponseBuilder.Default()
                .WithProducerRegistrationFee(284200)
                .WithProducerLateRegistrationFee(77200)
                .WithProducerClosedLoopRecyclingFee(hcClrFeePence)
                .WithTotalFee(1603500)
                .WithOutstandingPayment(1603500)
                .WithSubsidiariesFeeBreakdown(subsidiariesFeePence, subsidiaryClrFeePence, 2));

        var page = await LoadDetailsPage(submissionId);

        using (new AssertionScope())
        {
            page.SubTotal.Should().Be(16035.00m);
            AssertDirectProducerHcClrLine(page, units: 1, amountPence: hcClrFeePence);
            AssertDirectProducerSubsidiaryCompaniesLine(page, units: 3, amountPence: netSubsidiaryFeePence);
            AssertDirectProducerSubsidiaryClrLine(page, units: 2, amountPence: subsidiaryClrFeePence);
        }

        FacadeServer.ShouldHavePostedProducerRegistrationFee(new
        {
            applicationReferenceNumber = appRef,
            isClosedLoopRecycling = true,
            noOfHoldingCompaniesClosedLoopRecycling = 1,
            noOfSubsidiariesClosedLoopRecycling = 2,
        });
    }

    [Fact]
    [Trait("Scenario", "12")]
    public async Task Cso_3LargeMembers_HcClr2_SubClrOff_FlagsFirstTwoMembersOnly_AndDisplaysHcClrLine()
    {
        const int scenario = 12;
        var submissionId = ClrPaymentScenarioIds.Scenario12;
        var appRef = ClrPaymentScenarioIds.AppRef(scenario);
        const int hcClrFeePence = 254800;

        FacadeServer.SetupRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .WithApplicationReferenceNumber(appRef)
                .WithCsoMembers(
                    CsoMemberBuilder.Large("100001").WithHoldingCompanyClrCount(2),
                    CsoMemberBuilder.Large("100002"),
                    CsoMemberBuilder.Large("100003")));

        FacadeServer.SetupComplianceSchemeRegistrationFee(
            CompliancePaymentResponseBuilder.Default()
                .WithComplianceSchemeRegistrationFee(284200)
                .WithTotalFee(1200000)
                .WithMembers(
                    BuildCsoMemberWithHcClr("100001", hcClrFeePence),
                    BuildCsoMemberWithHcClr("100002", hcClrFeePence),
                    ComplianceSchemeMemberFeeBuilder.Default()
                        .WithMemberId("100003")
                        .WithTotalMemberFee(165800)));

        var page = await LoadDetailsPage(submissionId);

        using (new AssertionScope())
        {
            page.SubTotal.Should().Be(12000.00m);
            AssertCsoHcClrLine(page, units: 2, amountPence: hcClrFeePence * 2);
            page.FindSubsidiaryClrLine().Should().BeNull();
        }

        FacadeServer.ShouldHavePostedComplianceSchemeRegistrationFee(new
        {
            applicationReferenceNumber = appRef,
            complianceSchemeMembers = new[]
            {
                new { memberId = "100001", isClosedLoopRecycling = true, noOfHoldingCompaniesClosedLoopRecycling = 1 },
                new { memberId = "100002", isClosedLoopRecycling = true, noOfHoldingCompaniesClosedLoopRecycling = 1 },
                new { memberId = "100003", isClosedLoopRecycling = false, noOfHoldingCompaniesClosedLoopRecycling = 0 },
            },
        });
    }

    [Fact]
    [Trait("Scenario", "13")]
    public async Task Cso_2Large1Small_HcClr2_SubClrOff_ExcludesSmallMemberFromHcClr_AndDisplaysHcClrLine()
    {
        const int scenario = 13;
        var submissionId = ClrPaymentScenarioIds.Scenario13;
        var appRef = ClrPaymentScenarioIds.AppRef(scenario);
        const int hcClrFeePence = 254800;

        FacadeServer.SetupRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .WithApplicationReferenceNumber(appRef)
                .WithCsoMembers(
                    CsoMemberBuilder.Large("100001").WithHoldingCompanyClrCount(2),
                    CsoMemberBuilder.Large("100002"),
                    CsoMemberBuilder.Large("100003").WithMemberType("small")));

        FacadeServer.SetupComplianceSchemeRegistrationFee(
            CompliancePaymentResponseBuilder.Default()
                .WithComplianceSchemeRegistrationFee(284200)
                .WithTotalFee(900000)
                .WithMembers(
                    BuildCsoMemberWithHcClr("100001", hcClrFeePence),
                    BuildCsoMemberWithHcClr("100002", hcClrFeePence),
                    ComplianceSchemeMemberFeeBuilder.Default()
                        .WithMemberId("100003")
                        .WithMemberType("small")
                        .WithMemberRegistrationFee(48000)
                        .WithTotalMemberFee(48000)));

        var page = await LoadDetailsPage(submissionId);

        using (new AssertionScope())
        {
            page.SubTotal.Should().Be(9000.00m);
            AssertCsoHcClrLine(page, units: 2, amountPence: hcClrFeePence * 2);
            page.FindSubsidiaryClrLine().Should().BeNull();
        }

        FacadeServer.ShouldHavePostedComplianceSchemeRegistrationFee(new
        {
            applicationReferenceNumber = appRef,
            complianceSchemeMembers = new[]
            {
                new { memberId = "100001", memberType = "large", isClosedLoopRecycling = true },
                new { memberId = "100002", memberType = "large", isClosedLoopRecycling = true },
                new { memberId = "100003", memberType = "small", isClosedLoopRecycling = false },
            },
        });
    }

    [Fact]
    [Trait("Scenario", "14")]
    public async Task Cso_3Members_3SubsEach_HcClrOff_SubClrMixed_DisplaysAggregatedPartialSubClr_AndZerosSubClrInRequest()
    {
        const int scenario = 14;
        var submissionId = ClrPaymentScenarioIds.Scenario14;
        var appRef = ClrPaymentScenarioIds.AppRef(scenario);
        const int subsidiariesFeePence = 647600;
        const int memberOneClrFeePence = 509600;
        const int memberTwoClrFeePence = 169900;

        FacadeServer.SetupRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .WithApplicationReferenceNumber(appRef)
                .WithCsoMembers(
                    CsoMemberBuilder.Large("100001").WithSubsidiaries(3).WithSubsidiariesClosedLoopRecycling(3),
                    CsoMemberBuilder.Large("100002").WithSubsidiaries(3).WithSubsidiariesClosedLoopRecycling(1),
                    CsoMemberBuilder.Large("100003").WithSubsidiaries(3).WithSubsidiariesClosedLoopRecycling(0)));

        FacadeServer.SetupComplianceSchemeRegistrationFee(
            CompliancePaymentResponseBuilder.Default()
                .WithComplianceSchemeRegistrationFee(284200)
                .WithTotalFee(4000000)
                .WithMembers(
                    BuildCsoMemberWithSubsidiaryClrBreakdown("100001", subsidiariesFeePence, memberOneClrFeePence, 3),
                    BuildCsoMemberWithSubsidiaryClrBreakdown("100002", subsidiariesFeePence, memberTwoClrFeePence, 1),
                    BuildCsoMemberWithSubsidiaries("100003", 165800, subsidiariesFeePence)));

        var page = await LoadDetailsPage(submissionId);

        using (new AssertionScope())
        {
            page.FindCsoHoldingCompanyClrLine().Should().BeNull();
            AssertCsoSubsidiaryCompaniesLine(
                page,
                units: 9,
                amountPence: (subsidiariesFeePence - memberOneClrFeePence)
                             + (subsidiariesFeePence - memberTwoClrFeePence)
                             + subsidiariesFeePence);
            AssertCsoSubsidiaryClrLine(page, units: 4, amountPence: memberOneClrFeePence + memberTwoClrFeePence);
        }

        FacadeServer.ShouldHavePostedComplianceSchemeRegistrationFee(new
        {
            applicationReferenceNumber = appRef,
            complianceSchemeMembers = new[]
            {
                new { memberId = "100001", isClosedLoopRecycling = false, noOfSubsidiariesClosedLoopRecycling = 0 },
                new { memberId = "100002", isClosedLoopRecycling = false, noOfSubsidiariesClosedLoopRecycling = 0 },
                new { memberId = "100003", isClosedLoopRecycling = false, noOfSubsidiariesClosedLoopRecycling = 0 },
            },
        });
    }

    [Fact]
    [Trait("Scenario", "15")]
    public async Task Cso_3Members_3SubsEach_HcClr2_SubClrMixed_DisplaysBothClrTypes_AndZerosSubClrInRequest()
    {
        const int scenario = 15;
        var submissionId = ClrPaymentScenarioIds.Scenario15;
        var appRef = ClrPaymentScenarioIds.AppRef(scenario);
        const int hcClrFeePence = 254800;
        const int subsidiariesFeePence = 647600;
        const int memberOneSubClrFeePence = 509600;
        const int memberTwoSubClrFeePence = 169900;
        const int netSubsidiaryFeePence =
            (subsidiariesFeePence - memberOneSubClrFeePence)
            + (subsidiariesFeePence - memberTwoSubClrFeePence)
            + subsidiariesFeePence;

        FacadeServer.SetupRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .WithApplicationReferenceNumber(appRef)
                .WithCsoMembers(
                    CsoMemberBuilder.Large("100001")
                        .WithHoldingCompanyClrCount(2)
                        .WithSubsidiaries(3)
                        .WithSubsidiariesClosedLoopRecycling(3),
                    CsoMemberBuilder.Large("100002")
                        .WithSubsidiaries(3)
                        .WithSubsidiariesClosedLoopRecycling(1),
                    CsoMemberBuilder.Large("100003").WithSubsidiaries(3)));

        FacadeServer.SetupComplianceSchemeRegistrationFee(
            CompliancePaymentResponseBuilder.Default()
                .WithComplianceSchemeRegistrationFee(284200)
                .WithTotalFee(6000000)
                .WithMembers(
                    BuildCsoMemberWithHcAndSubsidiaryClr("100001", hcClrFeePence, subsidiariesFeePence, memberOneSubClrFeePence, 3),
                    BuildCsoMemberWithHcAndSubsidiaryClr("100002", hcClrFeePence, subsidiariesFeePence, memberTwoSubClrFeePence, 1),
                    BuildCsoMemberWithSubsidiaries("100003", 165800, subsidiariesFeePence)));

        var page = await LoadDetailsPage(submissionId);

        using (new AssertionScope())
        {
            AssertCsoHcClrLine(page, units: 2, amountPence: hcClrFeePence * 2);
            AssertCsoSubsidiaryClrLine(page, units: 4, amountPence: memberOneSubClrFeePence + memberTwoSubClrFeePence);
            AssertCsoSubsidiaryCompaniesLine(page, units: 9, amountPence: netSubsidiaryFeePence);
        }

        FacadeServer.ShouldHavePostedComplianceSchemeRegistrationFee(new
        {
            applicationReferenceNumber = appRef,
            complianceSchemeMembers = new[]
            {
                new { memberId = "100001", isClosedLoopRecycling = true, noOfSubsidiariesClosedLoopRecycling = 0 },
                new { memberId = "100002", isClosedLoopRecycling = true, noOfSubsidiariesClosedLoopRecycling = 0 },
                new { memberId = "100003", isClosedLoopRecycling = false, noOfSubsidiariesClosedLoopRecycling = 0 },
            },
        });
    }

    private async Task<ManageRegistrationSubmissionDetailsPageModel> LoadDetailsPage(Guid submissionId) =>
        await GetAsPageModel<ManageRegistrationSubmissionDetailsPageModel>(
            $"/regulators/registration-submission-details/{submissionId}");

    private static void AssertDirectProducerHasNoClrLines(ManageRegistrationSubmissionDetailsPageModel page)
    {
        page.FindDirectProducerHoldingCompanyClrLine().Should().BeNull();
        page.FindSubsidiaryClrLine().Should().BeNull();
    }

    private static void AssertCsoHasNoClrLines(ManageRegistrationSubmissionDetailsPageModel page)
    {
        page.FindCsoHoldingCompanyClrLine().Should().BeNull();
        page.FindSubsidiaryClrLine().Should().BeNull();
    }

    private static void AssertDirectProducerHcClrLine(
        ManageRegistrationSubmissionDetailsPageModel page,
        int units,
        int amountPence)
    {
        var line = page.FindDirectProducerHoldingCompanyClrLine();
        line.Should().NotBeNull();
        line!.Units.Should().Be(units);
        line.Amount.Should().Be(amountPence / 100m);
    }

    private static void AssertCsoHcClrLine(
        ManageRegistrationSubmissionDetailsPageModel page,
        int units,
        int amountPence)
    {
        var line = page.FindCsoHoldingCompanyClrLine();
        line.Should().NotBeNull();
        line!.Units.Should().Be(units);
        line.Amount.Should().Be(amountPence / 100m);
    }

    private static void AssertDirectProducerSubsidiaryCompaniesLine(
        ManageRegistrationSubmissionDetailsPageModel page,
        int units,
        int amountPence)
    {
        var line = page.FindSubsidiaryCompaniesLine();
        line.Should().NotBeNull();
        line!.Units.Should().Be(units);
        line.Amount.Should().Be(amountPence / 100m);
    }

    private static void AssertCsoSubsidiaryCompaniesLine(
        ManageRegistrationSubmissionDetailsPageModel page,
        int units,
        int amountPence)
    {
        AssertDirectProducerSubsidiaryCompaniesLine(page, units, amountPence);
    }

    private static void AssertDirectProducerSubsidiaryClrLine(
        ManageRegistrationSubmissionDetailsPageModel page,
        int units,
        int amountPence)
    {
        var line = page.FindSubsidiaryClrLine();
        line.Should().NotBeNull();
        line!.Units.Should().Be(units);
        line.Amount.Should().Be(amountPence / 100m);
    }

    private static void AssertCsoSubsidiaryClrLine(
        ManageRegistrationSubmissionDetailsPageModel page,
        int units,
        int amountPence) =>
        AssertDirectProducerSubsidiaryClrLine(page, units, amountPence);

    private static void AssertCsoLargeProducersLine(ManageRegistrationSubmissionDetailsPageModel page, int units)
    {
        var line = page.FindLargeProducersLine();
        line.Should().NotBeNull();
        line!.Units.Should().Be(units);
    }

    private static ComplianceSchemeMemberFeeBuilder BuildCsoMemberWithSubsidiaries(
        string memberId,
        int memberRegistrationFeePence,
        int subsidiariesFeePence) =>
        ComplianceSchemeMemberFeeBuilder.Default()
            .WithMemberId(memberId)
            .WithMemberRegistrationFee(memberRegistrationFeePence)
            .WithSubsidiariesFee(subsidiariesFeePence)
            .WithTotalMemberFee(memberRegistrationFeePence + subsidiariesFeePence);

    private static ComplianceSchemeMemberFeeBuilder BuildCsoMemberWithHcClr(string memberId, int hcClrFeePence) =>
        ComplianceSchemeMemberFeeBuilder.Default()
            .WithMemberId(memberId)
            .WithMemberClosedLoopRecyclingFee(hcClrFeePence)
            .WithTotalMemberFee(165800 + hcClrFeePence);

    private static ComplianceSchemeMemberFeeBuilder BuildCsoMemberWithSubsidiaryClrBreakdown(
        string memberId,
        int subsidiariesFeePence,
        int subsidiaryClrFeePence,
        int subsidiaryClrCount) =>
        ComplianceSchemeMemberFeeBuilder.Default()
            .WithMemberId(memberId)
            .WithMemberRegistrationFee(165800)
            .WithTotalMemberFee(165800 + subsidiariesFeePence)
            .WithSubsidiariesFeeBreakdown(subsidiariesFeePence, subsidiaryClrFeePence, subsidiaryClrCount);

    private static ComplianceSchemeMemberFeeBuilder BuildCsoMemberWithSubsidiaryClr(
        string memberId,
        int hcClrFeePence,
        int subsidiariesFeePence,
        int subsidiaryClrFeePence,
        int subsidiaryClrCount) =>
        ComplianceSchemeMemberFeeBuilder.Default()
            .WithMemberId(memberId)
            .WithMemberRegistrationFee(165800)
            .WithMemberClosedLoopRecyclingFee(hcClrFeePence)
            .WithTotalMemberFee(165800 + hcClrFeePence + subsidiariesFeePence)
            .WithSubsidiariesFeeBreakdown(subsidiariesFeePence, subsidiaryClrFeePence, subsidiaryClrCount);

    private static ComplianceSchemeMemberFeeBuilder BuildCsoMemberWithHcAndSubsidiaryClr(
        string memberId,
        int hcClrFeePence,
        int subsidiariesFeePence,
        int subsidiaryClrFeePence,
        int subsidiaryClrCount) =>
        BuildCsoMemberWithSubsidiaryClr(memberId, hcClrFeePence, subsidiariesFeePence, subsidiaryClrFeePence, subsidiaryClrCount);
}
