namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

using System.Diagnostics.CodeAnalysis;

using EPR.RegulatorService.Frontend.Core.Enums;

[ExcludeFromCodeCoverage]
public class ProducerPaymentDetailsViewModel : PaymentDetailsViewModel
{
    public decimal ApplicationProcessingFee { get; set; }

    public decimal LateRegistrationFee { get; set; }

    public decimal OnlineMarketplaceFee { get; set; }

    public decimal SubsidiaryFee { get; set; }

    public decimal SubsidiaryOnlineMarketPlaceFee { get; set; }

    public decimal SubTotal { get; set; }

    public decimal PreviousPaymentsReceived { get; set; }

    public decimal TotalOutstanding { get; set; }

    public string ProducerSize { get; set; }

    public int NumberOfSubsidiaries { get; set; }

    public int NumberOfSubsidiariesBeingOnlineMarketplace { get; set; }

    public RegistrationSubmissionStatus? ResubmissionStatus { get; set; }

    public RegistrationSubmissionStatus Status { get; set; }
}