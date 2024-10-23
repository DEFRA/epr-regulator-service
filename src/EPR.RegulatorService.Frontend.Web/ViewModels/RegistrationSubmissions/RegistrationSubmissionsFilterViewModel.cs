namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

using EPR.RegulatorService.Frontend.Core.Models.Registrations;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

public class RegistrationSubmissionsFilterViewModel
{
    private bool? _clearFilters = false;

    public string? OrganisationName { get; set; } = string.Empty;
    public string? OrganisationRef { get; set; } = string.Empty;

    public bool? IsOrganisationComplianceChecked { get; set; }
    public bool? IsOrganisationSmallChecked { get; set; }
    public bool? IsOrganisationLargeChecked { get; set; }
    public bool? IsStatusGrantedChecked { get; set; }
    public bool? IsStatusRefusedChecked { get; set; }
    public bool? IsStatusPendingChecked { get; set; }
    public bool? IsStatusQueriedChecked { get; set; }
    public bool? IsStatusUpdatedChecked { get; set; }
    public bool? IsStatusCancelledChecked { get; set; }
    public bool? Is2025Checked { get; set; }

    public int PageNumber { get; set; }
    public bool? ClearFilters
    {
        get => _clearFilters;
        set
        {
            _clearFilters = value;
            if (value.HasValue)
            {
                OrganisationName = OrganisationRef = null;
                IsOrganisationComplianceChecked = IsOrganisationLargeChecked = IsOrganisationSmallChecked = false;
                IsStatusCancelledChecked = IsStatusGrantedChecked = IsStatusPendingChecked = IsStatusQueriedChecked = IsStatusRefusedChecked = IsStatusUpdatedChecked = false;
                Is2025Checked = false;
            }
        }
    }
    public bool? IsFilterApplied { get; set; }

    public static implicit operator RegistrationSubmissionsFilterModel(RegistrationSubmissionsFilterViewModel viewModel) => new RegistrationSubmissionsFilterModel
    {
        Page = viewModel.PageNumber,
        RelevantYear = (bool)viewModel.Is2025Checked ? "2025" : null,
        OrganisationRef = !string.IsNullOrEmpty(viewModel.OrganisationRef) ? viewModel.OrganisationRef : null,
        OrganisationName = !string.IsNullOrEmpty(viewModel.OrganisationName) ? viewModel.OrganisationName : null,
        OrganisationType = string.Join(" ", new[]
                               {
                                   (bool)viewModel.IsOrganisationComplianceChecked ? Core.Enums.RegistrationSubmissionOrganisationType.compliance.ToString() : null,
                                   (bool)viewModel.IsOrganisationLargeChecked ? Core.Enums.RegistrationSubmissionOrganisationType.large.ToString() : null,
                                   (bool)viewModel.IsOrganisationSmallChecked ? Core.Enums.RegistrationSubmissionOrganisationType.small.ToString() : null
                               }.Where(x => !string.IsNullOrEmpty(x))),
        SubmissionStatus = string.Join(" ", new[]
                               {
                                    (bool)viewModel.IsStatusCancelledChecked ? Core.Enums.RegistrationSubmissionStatus.cancelled.ToString() : null,
                                    (bool)viewModel.IsStatusGrantedChecked ? Core.Enums.RegistrationSubmissionStatus.granted.ToString() : null,
                                    (bool)viewModel.IsStatusPendingChecked ? Core.Enums.RegistrationSubmissionStatus.pending.ToString() : null,
                                    (bool)viewModel.IsStatusQueriedChecked ? Core.Enums.RegistrationSubmissionStatus.queried.ToString() : null,
                                    (bool)viewModel.IsStatusRefusedChecked ? Core.Enums.RegistrationSubmissionStatus.refused.ToString() : null,
                                    (bool)viewModel.IsStatusUpdatedChecked ? Core.Enums.RegistrationSubmissionStatus.updated.ToString() : null
                               }.Where(x => !string.IsNullOrEmpty(x)))
    };

    public static implicit operator RegistrationSubmissionsFilterViewModel(RegistrationSubmissionsFilterModel model) => new RegistrationSubmissionsFilterViewModel
    {
        OrganisationName = model.OrganisationName,
        OrganisationRef = model.OrganisationRef,
        IsOrganisationComplianceChecked = model.OrganisationType != null && model.OrganisationType.ToLower().Contains("compliance"),
        IsOrganisationSmallChecked = model.OrganisationType != null && model.OrganisationType.ToLower().Contains("small"),
        IsOrganisationLargeChecked = model.OrganisationType != null && model.OrganisationType.ToLower().Contains("large"),
        IsStatusGrantedChecked = model.SubmissionStatus != null && model.SubmissionStatus.ToLower().Contains("granted"),
        IsStatusRefusedChecked = model.SubmissionStatus != null && model.SubmissionStatus.ToLower().Contains("refused"),
        IsStatusPendingChecked = model.SubmissionStatus != null && model.SubmissionStatus.ToLower().Contains("pending"),
        IsStatusQueriedChecked = model.SubmissionStatus != null && model.SubmissionStatus.ToLower().Contains("queried"),
        IsStatusUpdatedChecked = model.SubmissionStatus != null && model.SubmissionStatus.ToLower().Contains("updated"),
        IsStatusCancelledChecked = model.SubmissionStatus != null && model.SubmissionStatus.ToLower().Contains("cancelled"),
        Is2025Checked = model.RelevantYear != null && model.RelevantYear.ToLower().Contains("2025"),
        PageNumber = model.Page ?? 1
    };
}
