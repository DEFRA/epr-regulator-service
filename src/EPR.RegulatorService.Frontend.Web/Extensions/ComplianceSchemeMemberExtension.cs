namespace EPR.RegulatorService.Frontend.Web.Extensions;

using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions.FacadeCommonData;

internal static class ComplianceSchemeMemberExtension
{
    internal static (IList<ComplianceSchemeMember> largeProducers, IList<ComplianceSchemeMember> smallProducers) GetIndividualProducers(
        this List<ComplianceSchemeMember> complianceSchemeMembers, List<CsoMembershipDetailsDto> csoMembershipDetails)
    {
        IList<ComplianceSchemeMember> largeProducers = [];
        IList<ComplianceSchemeMember> smallProducers = [];

        foreach (var csoMembershipDetail in csoMembershipDetails)
        {
            //Filter the member based on the member id match between the req and res object
            var complianceSchemeMember = complianceSchemeMembers
                .Find(r => r.MemberId.Equals(csoMembershipDetail.MemberId, StringComparison.OrdinalIgnoreCase) && r.MemberFee > 0);

            //Check the member type from the request object to filter the large producers
            if (csoMembershipDetail.MemberType.Equals("Large", StringComparison.OrdinalIgnoreCase))
            {
                largeProducers.Add(complianceSchemeMember);
            }

            //Check the member type from the request object to filter the small producers
            if (csoMembershipDetail.MemberType.Equals("Small", StringComparison.OrdinalIgnoreCase))
            {
                smallProducers.Add(complianceSchemeMember);
            }
        }

        return (largeProducers, smallProducers);
    }

    internal static decimal GetFees(this IList<ComplianceSchemeMember> complianceSchemeMembers) =>
        complianceSchemeMembers.Sum(r => r.MemberFee);

    internal static IList<decimal> GetLateProducers(this List<ComplianceSchemeMember> complianceSchemeMembers) =>
        complianceSchemeMembers.Where(r => r.LateRegistrationFee > 0).Select(r => r.LateRegistrationFee).ToList();

    internal static IList<decimal> GetOnlineMarketPlaces(this List<ComplianceSchemeMember> complianceSchemeMembers) =>
        complianceSchemeMembers.Where(r => r.OnlineMarketPlaceFee > 0).Select(r => r.OnlineMarketPlaceFee).ToList();

    internal static IList<decimal> GetSubsidiariesCompanies(this List<ComplianceSchemeMember> complianceSchemeMembers) =>
        complianceSchemeMembers.Where(r => r.SubsidiaryFee > 0).Select(r => r.SubsidiaryFee).ToList();
}