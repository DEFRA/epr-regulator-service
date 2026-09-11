using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions.FacadeCommonData;
using EPR.RegulatorService.Frontend.Web.Extensions;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Extensions;

[TestClass]
public class ComplianceSchemeMemberExtensionTests
{
    [TestMethod]
    public void GetIndividualProducers_UsesMemberTypeFromMemberWhenSet_IgnoresCsoDetails()
    {
        var members = new List<ComplianceSchemeMember>
        {
            new() { MemberId = "ORG-1", MemberType = "Large", MemberFee = 500m },
        };

        var (large, small) = members.GetIndividualProducers(csoMembershipDetails: null);

        large.Should().ContainSingle().Which.MemberId.Should().Be("ORG-1");
        small.Should().BeEmpty();
    }

    [TestMethod]
    public void GetIndividualProducers_FallsBackToCsoMembershipDetailsWhenMemberTypeMissing()
    {
        var members = new List<ComplianceSchemeMember>
        {
            new() { MemberId = "ORG-1", MemberType = null, MemberFee = 250m },
        };
        var csoDetails = new List<CsoMembershipDetailsDto>
        {
            new() { MemberId = "org-1", MemberType = "Small" },
        };

        var (large, small) = members.GetIndividualProducers(csoDetails);

        large.Should().BeEmpty();
        small.Should().ContainSingle().Which.MemberId.Should().Be("ORG-1");
    }

    [TestMethod]
    public void GetIndividualProducers_SkipsMemberWhenTypeCannotBeResolvedFromEitherSource()
    {
        var members = new List<ComplianceSchemeMember>
        {
            new() { MemberId = "ORG-1", MemberType = null, MemberFee = 100m },
            new() { MemberId = "ORG-2", MemberType = string.Empty, MemberFee = 100m },
        };
        var csoDetails = new List<CsoMembershipDetailsDto>
        {
            new() { MemberId = "ORG-3", MemberType = "Large" },
        };

        var (large, small) = members.GetIndividualProducers(csoDetails);

        large.Should().BeEmpty();
        small.Should().BeEmpty();
    }

    [TestMethod]
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
    public void GetClosedLoopRecyclingFees_ReturnsFeesForMembersWithClosedLoopFees()
    {
        var members = new List<ComplianceSchemeMember>
        {
            new() { ClosedLoopRecyclingFee = 20_000m },
            new() { ClosedLoopRecyclingFee = 30_000m },
            new() { ClosedLoopRecyclingFee = 0m }
        };

        members.GetClosedLoopRecyclingFees().Should().BeEquivalentTo([20_000m, 30_000m]);
    }

    [TestMethod]
    public void GetClosedLoopRecyclingFees_ReturnsEmptyListWhenNoMembersHaveClosedLoopFees()
    {
        var members = new List<ComplianceSchemeMember>
        {
            new() { ClosedLoopRecyclingFee = 0m }
        };

        members.GetClosedLoopRecyclingFees().Should().BeEmpty();
    }

    [TestMethod]
    public void GetClosedLoopRecyclingFees_ReturnsEmptyListWhenMembersListIsEmpty()
    {
        var members = new List<ComplianceSchemeMember>();

        members.GetClosedLoopRecyclingFees().Should().BeEmpty();
    }

    [TestMethod]
    public void GetSubsidiariesOnlineMarketPlaceFees_SumsFeesAndTreatsNullBreakdownAsZero()
    {
        var members = new List<ComplianceSchemeMember>
        {
            new()
            {
                SubsidiariesFeeBreakdown = new SubsidiariesFeeBreakdownResponse
                {
                    SubsidiaryOnlineMarketPlaceFee = 20_000m
                }
            },
            new()
            {
                SubsidiariesFeeBreakdown = new SubsidiariesFeeBreakdownResponse
                {
                    SubsidiaryOnlineMarketPlaceFee = 30_000m
                }
            },
            new()
            {
                SubsidiariesFeeBreakdown = null
            }
        };

        members.GetSubsidiariesOnlineMarketPlaceFees().Should().Be(50_000m);
    }
}
