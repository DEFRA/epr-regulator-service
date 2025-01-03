namespace EPR.RegulatorService.Frontend.Core.Models.Submissions;

public class PackagingCompliancePaymentRequest
{
    public required string ApplicationReferenceNumber { get; set; }

    public int MemberCount { get; set; }

    public required string Regulator { get; set; }

    public DateTime SubmissionDate { get; set; }
}