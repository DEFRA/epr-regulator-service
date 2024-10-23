using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Web.ViewModels.Shared;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

[ExcludeFromCodeCoverage]
public class RegistrationSubmissionsListViewModel
{
    public IEnumerable<RegistrationSubmissionDetailsViewModel> PagedRegistrationSubmissions { get; set; }

    public PaginationNavigationModel PaginationNavigationModel { get; set; }

    public RegistrationSubmissionsFilterViewModel RegistrationsFilterModel { get; set; } = new RegistrationSubmissionsFilterViewModel();

    public RegistrationSubmissionsFilterModel CreateFilterModel(int? desiredPage) => new RegistrationSubmissionsFilterModel
    {
        Page = desiredPage ?? 1,
        RelevantYear = (bool)RegistrationsFilterModel.Is2025Checked ? "2025" : null,
        OrganisationRef = !string.IsNullOrEmpty(RegistrationsFilterModel.OrganisationRef) ? RegistrationsFilterModel.OrganisationRef : null,
        OrganisationName = !string.IsNullOrEmpty(RegistrationsFilterModel.OrganisationName) ? RegistrationsFilterModel.OrganisationName : null,
        OrganisationType = string.Join(" ", new[]
        {
            (bool)RegistrationsFilterModel.IsOrganisationComplianceChecked ? Core.Enums.RegistrationSubmissionOrganisationType.compliance.ToString() : null,
            (bool)RegistrationsFilterModel.IsOrganisationLargeChecked ? Core.Enums.RegistrationSubmissionOrganisationType.large.ToString() : null,
            (bool)RegistrationsFilterModel.IsOrganisationSmallChecked ? Core.Enums.RegistrationSubmissionOrganisationType.small.ToString() : null
        }.Where(x => !string.IsNullOrEmpty(x))),
        SubmissionStatus = string.Join(" ", new[]
        {
            (bool)RegistrationsFilterModel.IsStatusCancelledChecked ? Core.Enums.RegistrationSubmissionStatus.cancelled.ToString() : null,
            (bool)RegistrationsFilterModel.IsStatusGrantedChecked ? Core.Enums.RegistrationSubmissionStatus.granted.ToString() : null,
            (bool)RegistrationsFilterModel.IsStatusPendingChecked ? Core.Enums.RegistrationSubmissionStatus.pending.ToString() : null,
            (bool)RegistrationsFilterModel.IsStatusQueriedChecked ? Core.Enums.RegistrationSubmissionStatus.queried.ToString() : null,
            (bool)RegistrationsFilterModel.IsStatusRefusedChecked ? Core.Enums.RegistrationSubmissionStatus.refused.ToString() : null,
            (bool)RegistrationsFilterModel.IsStatusUpdatedChecked ? Core.Enums.RegistrationSubmissionStatus.updated.ToString() : null
        }.Where(x => !string.IsNullOrEmpty(x)))
    };
}
