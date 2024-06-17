namespace EPR.RegulatorService.Frontend.Web.ViewModels.Shared;

public class RegulatorRegistrationFiltersModel
{
    public string? SearchOrganisationName { get; set; }
    public string? SearchOrganisationId { get; set; }
    public int[] SearchSubmissionYears { get; set; }
    public string[] SearchSubmissionPeriods { get; set; }
    public bool IsDirectProducerChecked { get; set; }
    public bool IsComplianceSchemeChecked { get; set; }
    public bool IsPendingRegistrationChecked { get; set; }
    public bool IsAcceptedRegistrationChecked { get; set; }
    public bool IsRejectedRegistrationChecked { get; set; }
    public int[] SubmissionYears { get; set; }
    public string[] SubmissionPeriods { get; set; }
}