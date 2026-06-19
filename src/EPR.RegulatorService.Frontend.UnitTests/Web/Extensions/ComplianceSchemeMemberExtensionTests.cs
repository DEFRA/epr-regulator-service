using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Web.Extensions;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Extensions;

[TestClass]
public class ComplianceSchemeMemberExtensionTests
{
    [TestMethod]
    [Ignore("Temporary: SubsidiariesFeeBreakdown excluded from GetNetSubsidiariesCompanyFees")]
    public void GetNetSubsidiariesCompanyFees_SumsNetFeesForMembersWithSubsidiaryFee()
    {
        var members = new List<ComplianceSchemeMember>
        {
            new()
            {
                SubsidiaryFee = 100_000m,
                SubsidiariesFeeBreakdown = new SubsidiariesFeeBreakdownResponse
                {
                    SubsidiaryOnlineMarketPlaceFee = 10_000m,
                    TotalSubsidiariesClosedLoopRecyclingFees = 15_000m
                }
            },
            new()
            {
                SubsidiaryFee = 0m,
                SubsidiariesFeeBreakdown = new SubsidiariesFeeBreakdownResponse
                {
                    SubsidiaryOnlineMarketPlaceFee = 5_000m
                }
            },
            new()
            {
                SubsidiaryFee = 50_000m,
                SubsidiariesFeeBreakdown = null
            }
        };

        members.GetNetSubsidiariesCompanyFees().Should().Be(125_000m);
    }

    [TestMethod]
    public void GetSubsidiariesClosedLoopRecyclingCount_SumsCountsAndTreatsNullBreakdownAsZero()
    {
        var members = new List<ComplianceSchemeMember>
        {
            new()
            {
                SubsidiariesFeeBreakdown = new SubsidiariesFeeBreakdownResponse
                {
                    CountOfClosedLoopRecyclingSubsidiaries = 2
                }
            },
            new()
            {
                SubsidiariesFeeBreakdown = new SubsidiariesFeeBreakdownResponse
                {
                    CountOfClosedLoopRecyclingSubsidiaries = 3
                }
            },
            new()
            {
                SubsidiariesFeeBreakdown = null
            }
        };

        members.GetSubsidiariesClosedLoopRecyclingCount().Should().Be(5);
    }

    [TestMethod]
    public void GetSubsidiariesClosedLoopRecyclingFees_SumsFeesAndTreatsNullBreakdownAsZero()
    {
        var members = new List<ComplianceSchemeMember>
        {
            new()
            {
                SubsidiariesFeeBreakdown = new SubsidiariesFeeBreakdownResponse
                {
                    TotalSubsidiariesClosedLoopRecyclingFees = 20_000m
                }
            },
            new()
            {
                SubsidiariesFeeBreakdown = new SubsidiariesFeeBreakdownResponse
                {
                    TotalSubsidiariesClosedLoopRecyclingFees = 30_000m
                }
            },
            new()
            {
                SubsidiariesFeeBreakdown = null
            }
        };

        members.GetSubsidiariesClosedLoopRecyclingFees().Should().Be(50_000m);
    }

    [TestMethod]
    public void GetClosedLoopRecyclingFee_ReturnsMaxFeeWhenMembersHaveClosedLoopFees()
    {
        var members = new List<ComplianceSchemeMember>
        {
            new() { ClosedLoopRecyclingFee = 20_000m },
            new() { ClosedLoopRecyclingFee = 30_000m },
            new() { ClosedLoopRecyclingFee = 0m }
        };

        members.GetClosedLoopRecyclingFee().Should().Be(30_000m);
    }

    [TestMethod]
    public void GetClosedLoopRecyclingFee_ReturnsZeroWhenNoMembersHaveClosedLoopFees()
    {
        var members = new List<ComplianceSchemeMember>
        {
            new() { ClosedLoopRecyclingFee = 0m }
        };

        members.GetClosedLoopRecyclingFee().Should().Be(0m);
    }

    [TestMethod]
    public void GetClosedLoopRecyclingFee_ReturnsZeroWhenMembersListIsEmpty()
    {
        var members = new List<ComplianceSchemeMember>();

        members.GetClosedLoopRecyclingFee().Should().Be(0m);
    }
}
