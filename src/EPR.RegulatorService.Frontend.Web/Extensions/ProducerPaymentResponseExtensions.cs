namespace EPR.RegulatorService.Frontend.Web.Extensions;

using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Web.Helpers;

internal static class ProducerPaymentResponseExtensions
{
    internal static decimal GetNetSubsidiaryCompaniesFee(this ProducerPaymentResponse response) =>
        SubsidiaryFeeHelper.GetNetSubsidiaryCompaniesFee(
            response.SubsidiaryFee,
            response.SubsidiariesFeeBreakdown);
}
