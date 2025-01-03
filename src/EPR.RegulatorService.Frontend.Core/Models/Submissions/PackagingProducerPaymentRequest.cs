namespace EPR.RegulatorService.Frontend.Core.Models.Submissions;

public class PackagingProducerPaymentRequest
{
    public required string ApplicationReferenceNumber { get; set; }

    public required string Regulator { get; set; }

    public required DateTime SubmissionDate { get; set; }
}