namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
public class RegistrationSubmissionsOrganisationPaymentDetails
{
    public decimal ApplicationProcessingFee { get; set; }

    public decimal OnlineMarketplaceFee { get; set; }

    public decimal SubsidiaryFee { get; set; }

    public decimal TotalChargeableItems => ApplicationProcessingFee + OnlineMarketplaceFee + SubsidiaryFee;

    public decimal PreviousPaymentsReceived { get; set; }

    public decimal TotalOutstanding => TotalChargeableItems - PreviousPaymentsReceived;

    public decimal? OfflinePaymentAmount { get; set; }
}
