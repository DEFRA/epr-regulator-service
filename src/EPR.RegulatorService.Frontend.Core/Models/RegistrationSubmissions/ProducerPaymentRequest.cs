using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

[ExcludeFromCodeCoverage]
public class ProducerPaymentRequest
{
    public required string ApplicationReferenceNumber { get; set; }

    public int NoOfSubsidiariesOnlineMarketplace { get; set; }

    public int NumberOfSubsidiaries { get; set; }

    public int NumberOfLateSubsidiaries { get; set; }

    public bool IsLateFeeApplicable { get; set; }

    public bool IsProducerOnlineMarketplace { get; set; }

    public required string ProducerType { get; set; }

    public required string Regulator { get; set; }

    public DateTime SubmissionDate { get; set; }
}