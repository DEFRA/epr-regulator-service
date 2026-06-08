namespace EPR.RegulatorService.Frontend.Web.Extensions;

using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

internal static class ProducerPaymentResponseExtensions
{
    /// <summary>
    /// Returns the subsidiary companies registration fee.
    /// The payment API returns <see cref="ProducerPaymentResponse.SubsidiaryFee"/> as a rollup that includes
    /// online marketplace and closed loop recycling fees, which are displayed as separate line items.
    /// </summary>
    internal static decimal GetSubsidiaryCompaniesFee(this ProducerPaymentResponse response)
    {
        var breakdown = response.SubsidiariesFeeBreakdown;
        return response.SubsidiaryFee
            - breakdown.SubsidiaryOnlineMarketPlaceFee
            - breakdown.TotalSubsidiariesClosedLoopRecyclingFees;
    }
}
