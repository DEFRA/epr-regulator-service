namespace EPR.RegulatorService.Frontend.Core.Models.Submissions;

public class PackagingCompliancePaymentRequest
{
    public required string ReferenceNumber { get; set; }

    public int MemberCount { get; set; }

    public required string Regulator { get; set; }

    public DateTime ResubmissionDate { get; set; }
}