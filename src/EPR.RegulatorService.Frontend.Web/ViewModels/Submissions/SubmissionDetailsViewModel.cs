namespace EPR.RegulatorService.Frontend.Web.ViewModels.Submissions;

public class SubmissionDetailsViewModel
{
    public string OrganisationName { get; set; }
    public string OrganisationType { get; set; }
    public string OrganisationReferenceNumber { get; set; }
    public string FormattedTimeAndDateOfSubmission { get; set; }
    public Guid SubmissionId { get; set; }
    public string SubmittedBy { get; set; }
    public string AccountRole { get; set; }
    public string Telephone { get; set; }
    public string Email { get; set; }
    public string Status { get; set; }
    public string PowerBiLogin { get; set; }
    public bool IsResubmission { get; set; }
    public string RejectionReason { get; set; }
    public bool ResubmissionRequired { get; set; }
    public string PreviousRejectionComments { get; set; }
}