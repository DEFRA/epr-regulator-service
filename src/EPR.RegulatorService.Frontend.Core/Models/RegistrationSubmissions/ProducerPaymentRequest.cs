namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

public class ProducerPaymentRequest
{
    public required string ApplicationReferenceNumber { get; set; }

    public int NoOfSubsidiariesOnlineMarketplace { get; set; }

    public int NumberOfSubsidiaries { get; set; }

    public bool IsLateFeeApplicable { get; set; }

    public int NoOfHoldingCompaniesClosedLoopRecycling { get; set; }

    public bool IsClosedLoopRecycling { get; set; }

    public int NoOfSubsidiariesClosedLoopRecycling { get; set; }

    public bool IsProducerOnlineMarketplace { get; set; }

    public required string ProducerType { get; set; }

    public required string Regulator { get; set; }

    public DateTime SubmissionDate { get; set; }
}