namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

public class RegistrationSubmissionsFilterViewModel
{
    public string? OrganisationName { get; set; } = string.Empty;
    public string? OrganisationRef { get; set; } = string.Empty;

    public bool IsOrganisationComplianceChecked { get; set; }
    public bool IsOrganisationSmallChecked { get; set; }
    public bool IsOrganisationLargeChecked { get; set; }
    public bool IsStatusGrantedChecked { get; set; }
    public bool IsStatusRefusedChecked { get; set; }
    public bool IsStatusPendingChecked { get; set; }
    public bool IsStatusQueriedChecked { get; set; }
    public bool IsStatusUpdatedChecked { get; set; }
    public bool IsStatusCancelledChecked { get; set; }
    public bool Is2025Checked { get; set; }
}
