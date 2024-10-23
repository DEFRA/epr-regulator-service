namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class PaymentDetailsViewModel
    {
        public decimal ApplicationProcessingFee { get; set; }

        public decimal OnlineMarketplaceFee { get; set; }

        public decimal SubsidiaryFee { get; set; }

        public decimal TotalChargeableItems => ApplicationProcessingFee + OnlineMarketplaceFee + SubsidiaryFee;

        public decimal PreviousPaymentsReceived { get; set; }

        public decimal TotalOutstanding => TotalChargeableItems - PreviousPaymentsReceived;

        public decimal OfflinePayment { get; set; }
    }
}
