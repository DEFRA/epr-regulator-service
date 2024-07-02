namespace EPR.RegulatorService.Frontend.Web.ViewModels.Shared;

public class RegulatorSubmissionFiltersModel
{
    public string? SearchOrganisationName { get; set; }
    public string? SearchOrganisationId { get; set; }
    public bool IsDirectProducerChecked { get; set; }
    public bool IsComplianceSchemeChecked { get; set; }
    public bool IsPendingSubmissionChecked { get; set; }
    public bool IsAcceptedSubmissionChecked { get; set; }
    public bool IsRejectedSubmissionChecked { get; set; }
    public int[] SearchSubmissionYears { get; set; }
    public string[] SearchSubmissionPeriods { get; set; }
    public int[] SubmissionYears { get; set; }
    public string[] SubmissionPeriods { get; set; }
}