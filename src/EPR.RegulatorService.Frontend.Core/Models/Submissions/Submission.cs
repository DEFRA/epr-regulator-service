namespace EPR.RegulatorService.Frontend.Core.Models.Submissions;

using Enums;

public class Submission : AbstractSubmission
{
    public bool IsResubmissionRequired { get; set; }
    public Guid FileId { get; set; }
    public string? ProducerType { get; set; }
    public Guid? ComplianceSchemeId { get; set; }
}