namespace EPR.RegulatorService.Frontend.Core.Models.Submissions;

public class Submission : AbstractSubmission
{
    public bool IsResubmissionRequired { get; set; }
    public Guid FileId { get; set; }
    public string? ProducerType { get; set; }
    public Guid? ComplianceSchemeId { get; set; }
    public string? ActualSubmissionPeriod { get; set; }
    public string PomFileName { get; set; }
    public string PomBlobName { get; set; }
}