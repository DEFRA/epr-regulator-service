namespace EPR.RegulatorService.Frontend.Core.Models.Submissions;

public class PackagingProducerPaymentRequest
{
    public required string ReferenceNumber { get; set; }

    public required string Regulator { get; set; }

    public required DateTime ResubmissionDate { get; set; }
}