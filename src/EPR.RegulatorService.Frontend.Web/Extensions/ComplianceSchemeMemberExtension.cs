namespace EPR.RegulatorService.Frontend.Web.Extensions;

using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions.FacadeCommonData;
using EPR.RegulatorService.Frontend.Web.Helpers;

internal static class ComplianceSchemeMemberExtension
{
    internal static (IList<ComplianceSchemeMember> largeProducers, IList<ComplianceSchemeMember> smallProducers) GetIndividualProducers(
        this List<ComplianceSchemeMember> complianceSchemeMembers, List<CsoMembershipDetailsDto> csoMembershipDetails)
    {
        IList<ComplianceSchemeMember> largeProducers = [];
        IList<ComplianceSchemeMember> smallProducers = [];

        foreach (var complianceSchemeMember in complianceSchemeMembers)
        {
            var memberType = ResolveMemberType(complianceSchemeMember, csoMembershipDetails);
            if (string.IsNullOrEmpty(memberType))
            {
                continue;
            }

            if (memberType.Equals("Large", StringComparison.OrdinalIgnoreCase) && complianceSchemeMember.MemberFee > 0)
            {
                largeProducers.Add(complianceSchemeMember);
            }
            else if (memberType.Equals("Small", StringComparison.OrdinalIgnoreCase) && complianceSchemeMember.MemberFee > 0)
            {
                smallProducers.Add(complianceSchemeMember);
            }
        }

        return (largeProducers, smallProducers);
    }

    private static string ResolveMemberType(
        ComplianceSchemeMember complianceSchemeMember,
        List<CsoMembershipDetailsDto> csoMembershipDetails)
    {
        if (!string.IsNullOrEmpty(complianceSchemeMember.MemberType))
        {
            return complianceSchemeMember.MemberType;
        }

        var matched = csoMembershipDetails?.Find(
            c => c.MemberId.Equals(complianceSchemeMember.MemberId, StringComparison.OrdinalIgnoreCase));
        return matched?.MemberType;
    }

    internal static decimal GetFees(this IList<ComplianceSchemeMember> complianceSchemeMembers) =>
        complianceSchemeMembers.Sum(r => r.MemberFee);

    internal static IList<decimal> GetLateProducers(this List<ComplianceSchemeMember> complianceSchemeMembers) =>
        complianceSchemeMembers.Where(r => r.LateRegistrationFee > 0).Select(r => r.LateRegistrationFee).ToList();

    internal static IList<decimal> GetOnlineMarketPlaces(this List<ComplianceSchemeMember> complianceSchemeMembers) =>
        complianceSchemeMembers.Where(r => r.OnlineMarketPlaceFee > 0).Select(r => r.OnlineMarketPlaceFee).ToList();

    internal static decimal GetNetSubsidiariesCompanyFees(this List<ComplianceSchemeMember> complianceSchemeMembers) =>
        complianceSchemeMembers
            .Where(r => r.SubsidiaryFee > 0)
            .Sum(r => SubsidiaryFeeHelper.GetNetSubsidiaryCompaniesFee(
                r.SubsidiaryFee,
                r.SubsidiariesFeeBreakdown));

    internal static int GetSubsidiariesClosedLoopRecyclingCount(this List<ComplianceSchemeMember> complianceSchemeMembers) =>
        complianceSchemeMembers.Sum(r => r.SubsidiariesFeeBreakdown?.CountOfClosedLoopRecyclingSubsidiaries ?? 0);

    internal static decimal GetSubsidiariesClosedLoopRecyclingFees(this List<ComplianceSchemeMember> complianceSchemeMembers) =>
        complianceSchemeMembers.Sum(r => r.SubsidiariesFeeBreakdown?.TotalSubsidiariesClosedLoopRecyclingFees ?? 0);

    internal static IList<decimal> GetClosedLoopRecyclingFees(this List<ComplianceSchemeMember> complianceSchemeMembers) =>
        complianceSchemeMembers.Where(r => r.ClosedLoopRecyclingFee > 0).Select(r => r.ClosedLoopRecyclingFee).ToList();

    internal static decimal GetSubsidiariesOnlineMarketPlaceFees(this List<ComplianceSchemeMember> complianceSchemeMembers) =>
        complianceSchemeMembers.Sum(r => r.SubsidiariesFeeBreakdown?.SubsidiaryOnlineMarketPlaceFee ?? 0);

    internal static int GetSubsidiariesOnlineMarketPlaceCount(this List<ComplianceSchemeMember> complianceSchemeMembers) =>
        complianceSchemeMembers.Sum(r => r.SubsidiariesFeeBreakdown?.OnlineMarketPlaceSubsidiariesCount ?? 0);
}