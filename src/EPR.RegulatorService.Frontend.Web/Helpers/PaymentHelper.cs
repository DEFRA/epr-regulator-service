namespace EPR.RegulatorService.Frontend.Web.Helpers;
internal static class PaymentHelper
{
    internal static decimal GetUpdatedTotalOutstanding(decimal currentValue)
        => currentValue < 0.00M ? 0.00M : currentValue;
}