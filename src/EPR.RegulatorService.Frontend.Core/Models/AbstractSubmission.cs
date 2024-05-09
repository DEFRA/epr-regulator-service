namespace EPR.RegulatorService.Frontend.Core.Models;

using Enums;

public class AbstractSubmission
{
    public Guid SubmissionId { get; set; }
    public DateTime SubmittedDate { get; set; }
    public string? Decision { get; set; } = string.Empty;
    public bool IsResubmission { get; set; }
    public Guid OrganisationId { get; set; }
    public string? OrganisationName { get; set; }
    public string? OrganisationReference { get; set; }
    public OrganisationType OrganisationType { get; set; }
    public Guid? UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Telephone { get; set; }
    public string? ServiceRole { get; set; }
    public string? SubmissionPeriod { get; set; }
    public string? Comments { get; set; }
    public string? PreviousRejectionComments { get; set; }
}