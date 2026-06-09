namespace EPR.RegulatorService.Frontend.Web.Extensions;

using Core.Models.RegistrationSubmissions;

internal static class SubsidiaryFeeExtensions
{
    /// <summary>
    /// Returns the net subsidiary companies registration fee.
    /// The payment API returns subsidiary fees as a rollup that includes
    /// online marketplace and closed loop recycling fees, which are displayed as separate line items.
    /// </summary>
    internal static decimal GetNetSubsidiaryCompaniesFee(
        decimal subsidiaryFee,
        SubsidiariesFeeBreakdownResponse? breakdown)
    {
        if (breakdown is null)
        {
            return subsidiaryFee;
        }

        return subsidiaryFee
            - breakdown.SubsidiaryOnlineMarketPlaceFee
            - breakdown.TotalSubsidiariesClosedLoopRecyclingFees;
    }
}
