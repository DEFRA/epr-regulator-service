namespace EPR.RegulatorService.Frontend.Web.Helpers;
internal static class PaymentHelper
{
    internal static decimal GetUpdatedTotalOutstanding(decimal currentValue, bool showZeroFeeForTotalOutstanding) =>
        showZeroFeeForTotalOutstanding && currentValue < 0.00M ? 0.00M : currentValue;
}