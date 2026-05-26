namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

public class ProducerPaymentRequest
{
    public required string ApplicationReferenceNumber { get; set; }

    public int NumberOfSubsidiariesOnlineMarketplace { get; set; }

    public int NumberOfSubsidiaries { get; set; }

    public bool IsLateFeeApplicable { get; set; }

    public bool IsClosedLoopRecycling  { get; set; }

    public int NumberOfSubsidiariesClosedLoopRecycling { get; set; }

    public bool IsProducerOnlineMarketplace { get; set; }

    public required string ProducerType { get; set; }

    public required string Regulator { get; set; }

    public DateTime SubmissionDate { get; set; }
}