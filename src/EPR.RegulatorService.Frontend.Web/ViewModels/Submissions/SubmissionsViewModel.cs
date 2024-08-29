namespace EPR.RegulatorService.Frontend.Web.ViewModels.Submissions;

using Core.Models;

public class SubmissionsViewModel
{
    public int? PageNumber { get; set; }
    public string PowerBiLogin { get; set; }
    public int[] SubmissionYears { get; set; }
    public string[] SubmissionPeriods { get; set; }
    public EndpointResponseStatus? RejectSubmissionResult { get; set; } = EndpointResponseStatus.NotSet;
    public EndpointResponseStatus? AcceptSubmissionResult { get; set; } = EndpointResponseStatus.NotSet;
    public string? OrganisationName { get; set; }
    public string? SearchOrganisationName { get; set; } = string.Empty;
    public string? SearchOrganisationId { get; set; } = string.Empty;
    public bool IsDirectProducerChecked { get; set; }
    public bool IsComplianceSchemeChecked { get; set; }
    public bool IsPendingSubmissionChecked { get; set; }
    public bool IsAcceptedSubmissionChecked { get; set; }
    public bool IsRejectedSubmissionChecked { get; set; }
    public int[] SearchSubmissionYears { get; set; }
    public string[] SearchSubmissionPeriods { get; set; }
}