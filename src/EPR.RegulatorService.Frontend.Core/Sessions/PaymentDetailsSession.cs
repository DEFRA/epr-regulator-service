namespace EPR.RegulatorService.Frontend.Core.Sessions
{
    public class PaymentDetailsSession
    {
        public decimal ApplicationFee { get; set; }

        public int SchemeMemberCount { get; set; }

        public int SmallProducerCount { get; set; }

        public decimal SmallProducerFee { get; set; }

        public int LargeProducerCount { get; set; }

        public decimal LargeProducerFee { get; set; }

        public int OnlineMarketPlaceCount { get; set; }

        public decimal OnlineMarketPlaceFee { get; set; }

        public int SubsidiariesCompanyCount { get; set; }

        public decimal SubsidiariesCompanyFee { get; set; }

        public int LateProducerCount { get; set; }

        public decimal LateProducerFee { get; set; }

        public decimal SubTotal { get; set; }

        public decimal PreviousPaymentReceived { get; set; }

        public decimal TotalOutstanding { get; set; }

        public bool IsComplianceSchemeSelected { get; set; }

        public bool IsProducerSelected { get; set; }
    }
}