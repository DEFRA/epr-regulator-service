namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    public class PaymentDetailsViewModel
    {
        public string SubmissionPeriod { get; set; }

        public decimal ApplicationProcessingFee { get; set; }

        public decimal OnlineMarketplaceFee { get; set; }

        public decimal SubsidiaryFee { get; set; }

        public decimal TotalChargeableItems => ApplicationProcessingFee + OnlineMarketplaceFee + SubsidiaryFee;

        public decimal PreviousPaymentsReceived { get; set; }

        public decimal TotalOutstanding => TotalChargeableItems - PreviousPaymentsReceived;
    }
}
