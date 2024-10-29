namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

using System.Globalization;

using EPR.RegulatorService.Frontend.Core.Models.Registrations;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

public class RegistrationSubmissionsFilterViewModel
{
    private bool _clearFilters = false;

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

    public int PageNumber { get; set; } = 1;
    public int? PageSize { get; set; }

    public bool ClearFilters
    {
        get => _clearFilters;
        set
        {
            _clearFilters = value;
            if (value)
            {
                OrganisationName = OrganisationRef = null;
                IsOrganisationComplianceChecked = IsOrganisationLargeChecked = IsOrganisationSmallChecked = false;
                IsStatusCancelledChecked = IsStatusGrantedChecked = IsStatusPendingChecked = IsStatusQueriedChecked = IsStatusRefusedChecked = IsStatusUpdatedChecked = false;
                Is2025Checked = false;
            }
        }
    }

    public bool IsFilterApplied { get; set; }

    public static implicit operator RegistrationSubmissionsFilterModel(RegistrationSubmissionsFilterViewModel viewModel) => new RegistrationSubmissionsFilterModel
    {
        Page = viewModel.PageNumber,
        PageSize = viewModel.PageSize,
        RelevantYear = viewModel.Is2025Checked ? "2025" : null,
        OrganisationRef = !string.IsNullOrEmpty(viewModel.OrganisationRef) ? viewModel.OrganisationRef : null,
        OrganisationName = !string.IsNullOrEmpty(viewModel.OrganisationName) ? viewModel.OrganisationName : null,
        OrganisationType = string.Join(" ", new[]
                               {
                                   viewModel.IsOrganisationComplianceChecked ? Core.Enums.RegistrationSubmissionOrganisationType.compliance.ToString() : null,
                                   viewModel.IsOrganisationLargeChecked ? Core.Enums.RegistrationSubmissionOrganisationType.large.ToString() : null,
                                   viewModel.IsOrganisationSmallChecked ? Core.Enums.RegistrationSubmissionOrganisationType.small.ToString() : null
                               }.Where(x => !string.IsNullOrEmpty(x))),
        SubmissionStatus = string.Join(" ", new[]
                               {
                                    viewModel.IsStatusCancelledChecked ? Core.Enums.RegistrationSubmissionStatus.cancelled.ToString() : null,
                                    viewModel.IsStatusGrantedChecked ? Core.Enums.RegistrationSubmissionStatus.granted.ToString() : null,
                                    viewModel.IsStatusPendingChecked ? Core.Enums.RegistrationSubmissionStatus.pending.ToString() : null,
                                    viewModel.IsStatusQueriedChecked ? Core.Enums.RegistrationSubmissionStatus.queried.ToString() : null,
                                    viewModel.IsStatusRefusedChecked ? Core.Enums.RegistrationSubmissionStatus.refused.ToString() : null,
                                    viewModel.IsStatusUpdatedChecked ? Core.Enums.RegistrationSubmissionStatus.updated.ToString() : null
                               }.Where(x => !string.IsNullOrEmpty(x)))
    };

    public static implicit operator RegistrationSubmissionsFilterViewModel(RegistrationSubmissionsFilterModel model) => new RegistrationSubmissionsFilterViewModel
    {
        OrganisationName = model.OrganisationName,
        OrganisationRef = model.OrganisationRef,
        IsOrganisationComplianceChecked = model.OrganisationType != null && model.OrganisationType.Contains("compliance", StringComparison.OrdinalIgnoreCase),
        IsOrganisationSmallChecked = model.OrganisationType != null && model.OrganisationType.Contains("small", StringComparison.OrdinalIgnoreCase),
        IsOrganisationLargeChecked = model.OrganisationType != null && model.OrganisationType.Contains("large", StringComparison.OrdinalIgnoreCase),
        IsStatusGrantedChecked = model.SubmissionStatus != null && model.SubmissionStatus.Contains("granted", StringComparison.OrdinalIgnoreCase),
        IsStatusRefusedChecked = model.SubmissionStatus != null && model.SubmissionStatus.Contains("refused", StringComparison.OrdinalIgnoreCase),
        IsStatusPendingChecked = model.SubmissionStatus != null && model.SubmissionStatus.Contains("pending", StringComparison.OrdinalIgnoreCase),
        IsStatusQueriedChecked = model.SubmissionStatus != null && model.SubmissionStatus.Contains("queried", StringComparison.OrdinalIgnoreCase),
        IsStatusUpdatedChecked = model.SubmissionStatus != null && model.SubmissionStatus.Contains("updated", StringComparison.OrdinalIgnoreCase),
        IsStatusCancelledChecked = model.SubmissionStatus != null && model.SubmissionStatus.Contains("cancelled", StringComparison.OrdinalIgnoreCase),
        Is2025Checked = model.RelevantYear != null && model.RelevantYear.Contains("2025", StringComparison.OrdinalIgnoreCase),
        PageNumber = model.Page ?? 1,
        PageSize = model.PageSize ?? 20
    };
}
