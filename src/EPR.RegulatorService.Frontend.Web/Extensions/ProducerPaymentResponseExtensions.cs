namespace EPR.RegulatorService.Frontend.Web.Extensions;

using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

internal static class ProducerPaymentResponseExtensions
{
    internal static decimal GetNetSubsidiaryCompaniesFee(this ProducerPaymentResponse response) =>
        SubsidiaryFeeExtensions.GetNetSubsidiaryCompaniesFee(
            response.SubsidiaryFee,
            response.SubsidiariesFeeBreakdown);
}
