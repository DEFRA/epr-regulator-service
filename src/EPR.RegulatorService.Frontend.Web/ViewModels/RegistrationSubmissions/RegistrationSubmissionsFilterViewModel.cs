namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

public class RegistrationSubmissionsFilterViewModel
{
    private bool _clearFilters = false;

    public string? OrganisationName { get; set; } = string.Empty;
    public string? OrganisationRef { get; set; } = string.Empty;

    public bool IsOrganisationComplianceChecked { get; set; }
    public bool IsDirectProducerChecked { get; set; }
    public bool IsOrganisationSmallChecked { get; set; }
    public bool IsOrganisationLargeChecked { get; set; }
    public bool IsStatusGrantedChecked { get; set; }
    public bool IsStatusRefusedChecked { get; set; }
    public bool IsStatusPendingChecked { get; set; }
    public bool IsStatusQueriedChecked { get; set; }
    public bool IsStatusUpdatedChecked { get; set; }
    public bool IsStatusCancelledChecked { get; set; }

    public bool IsResubmissionPendingRegistrationChecked { get; set; }
    public bool IsResubmissionAcceptedRegistrationChecked { get; set; }
    public bool IsResubmissionRejectedRegistrationChecked { get; set; }

    public bool Show2026RelevantYearFilter { get; set; }

    public bool Is2025Checked { get; set; }
    public bool Is2026Checked { get; set; }

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
                IsOrganisationComplianceChecked = IsDirectProducerChecked =
                    IsOrganisationLargeChecked = IsOrganisationSmallChecked = false;
                IsStatusCancelledChecked = IsStatusGrantedChecked = IsStatusPendingChecked =
                    IsStatusQueriedChecked = IsStatusRefusedChecked = IsStatusUpdatedChecked = false;
                IsResubmissionPendingRegistrationChecked = IsResubmissionAcceptedRegistrationChecked =
                    IsResubmissionRejectedRegistrationChecked = false;
                Is2025Checked = false;
                Is2026Checked = false;
            }
        }
    }

    public bool IsFilterApplied { get; set; }
    public int NationId { get; set; }

    public static implicit operator
        RegistrationSubmissionsFilterModel(RegistrationSubmissionsFilterViewModel viewModel) => new()
    {
        PageNumber = viewModel.PageNumber,
        PageSize = viewModel.PageSize,
        NationId = viewModel.NationId,
        Show2026RelevantYearFilter = viewModel.Show2026RelevantYearFilter,
        RelevantYears =
            string.Join(", ",
                new[] { viewModel.Is2025Checked ? "2025" : null, viewModel.Is2026Checked ? "2026" : null }
                    .Where(year => !string.IsNullOrWhiteSpace(year))),
        OrganisationReference = !string.IsNullOrEmpty(viewModel.OrganisationRef) ? viewModel.OrganisationRef : null,
        OrganisationName = !string.IsNullOrEmpty(viewModel.OrganisationName) ? viewModel.OrganisationName : null,
        OrganisationType =
            string.Join(" ",
                new[]
                {
                    viewModel.IsOrganisationComplianceChecked
                        ? Core.Enums.RegistrationSubmissionOrganisationType.compliance.ToString()
                        : null,
                    viewModel.IsDirectProducerChecked
                        ? Core.Enums.RegistrationSubmissionOrganisationType.direct.ToString()
                        : null,
                    viewModel.IsOrganisationLargeChecked
                        ? Core.Enums.RegistrationSubmissionOrganisationType.large.ToString()
                        : null,
                    viewModel.IsOrganisationSmallChecked
                        ? Core.Enums.RegistrationSubmissionOrganisationType.small.ToString()
                        : null
                }.Where(x => !string.IsNullOrEmpty(x))),
        Statuses =
            string.Join(" ",
                new[]
                {
                    viewModel.IsStatusCancelledChecked
                        ? Core.Enums.RegistrationSubmissionStatus.Cancelled.ToString()
                        : null,
                    viewModel.IsStatusGrantedChecked
                        ? Core.Enums.RegistrationSubmissionStatus.Granted.ToString()
                        : null,
                    viewModel.IsStatusPendingChecked
                        ? Core.Enums.RegistrationSubmissionStatus.Pending.ToString()
                        : null,
                    viewModel.IsStatusQueriedChecked
                        ? Core.Enums.RegistrationSubmissionStatus.Queried.ToString()
                        : null,
                    viewModel.IsStatusRefusedChecked
                        ? Core.Enums.RegistrationSubmissionStatus.Refused.ToString()
                        : null,
                    viewModel.IsStatusUpdatedChecked
                        ? Core.Enums.RegistrationSubmissionStatus.Updated.ToString()
                        : null
                }.Where(x => !string.IsNullOrEmpty(x))),
        ResubmissionStatuses = string.Join(" ",
            new[]
            {
                viewModel.IsResubmissionPendingRegistrationChecked
                    ? Core.Enums.RegistrationSubmissionStatus.Pending.ToString()
                    : null,
                viewModel.IsResubmissionAcceptedRegistrationChecked
                    ? Core.Enums.RegistrationSubmissionStatus.Accepted.ToString()
                    : null,
                viewModel.IsResubmissionRejectedRegistrationChecked
                    ? Core.Enums.RegistrationSubmissionStatus.Rejected.ToString()
                    : null
            }.Where(x => !string.IsNullOrEmpty(x)))
    };

    public static implicit operator RegistrationSubmissionsFilterViewModel(RegistrationSubmissionsFilterModel model) =>
        new()
        {
            OrganisationName = model.OrganisationName,
            OrganisationRef = model.OrganisationReference,
            IsOrganisationComplianceChecked =
                model.OrganisationType != null &&
                model.OrganisationType.Contains("compliance", StringComparison.OrdinalIgnoreCase),
            IsDirectProducerChecked =
                model.OrganisationType != null &&
                model.OrganisationType.Contains("direct", StringComparison.OrdinalIgnoreCase),
            IsOrganisationSmallChecked =
                model.OrganisationType != null &&
                model.OrganisationType.Contains("small", StringComparison.OrdinalIgnoreCase),
            IsOrganisationLargeChecked =
                model.OrganisationType != null &&
                model.OrganisationType.Contains("large", StringComparison.OrdinalIgnoreCase),
            IsStatusGrantedChecked =
                model.Statuses != null && model.Statuses.Contains("granted", StringComparison.OrdinalIgnoreCase),
            IsStatusRefusedChecked =
                model.Statuses != null && model.Statuses.Contains("refused", StringComparison.OrdinalIgnoreCase),
            IsStatusPendingChecked =
                model.Statuses != null && model.Statuses.Contains("pending", StringComparison.OrdinalIgnoreCase),
            IsStatusQueriedChecked =
                model.Statuses != null && model.Statuses.Contains("queried", StringComparison.OrdinalIgnoreCase),
            IsStatusUpdatedChecked =
                model.Statuses != null && model.Statuses.Contains("updated", StringComparison.OrdinalIgnoreCase),
            IsStatusCancelledChecked =
                model.Statuses != null && model.Statuses.Contains("cancelled", StringComparison.OrdinalIgnoreCase),
            IsResubmissionPendingRegistrationChecked =
                model.ResubmissionStatuses != null &&
                model.ResubmissionStatuses.Contains("pending", StringComparison.OrdinalIgnoreCase),
            IsResubmissionAcceptedRegistrationChecked =
                model.ResubmissionStatuses != null &&
                model.ResubmissionStatuses.Contains("accepted", StringComparison.OrdinalIgnoreCase),
            IsResubmissionRejectedRegistrationChecked =
                model.ResubmissionStatuses != null &&
                model.ResubmissionStatuses.Contains("rejected", StringComparison.OrdinalIgnoreCase),
            Is2025Checked =
                model.RelevantYears != null &&
                model.RelevantYears.Contains("2025", StringComparison.OrdinalIgnoreCase),
            Is2026Checked =
                model.RelevantYears != null &&
                model.RelevantYears.Contains("2026", StringComparison.OrdinalIgnoreCase),
            PageNumber = model.PageNumber ?? 1,
            PageSize = model.PageSize ?? 20,
            NationId = model.NationId,
            Show2026RelevantYearFilter = model.Show2026RelevantYearFilter
        };
}