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

    public RegistrationSubmissionsFilterModel RegistrationsFilterModel { get; set; } = new RegistrationSubmissionsFilterModel();

    public RegistrationSubmissionsListViewModel()
    {
    }
}
