using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.Registrations;
using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.Registrations;

[ExcludeFromCodeCoverage]
public class RegistrationsViewModel
{
    public int? PageNumber { get; set; }
    public string PowerBiLogin { get; set; }
    public EndpointResponseStatus? RejectRegistrationResult { get; set; } = EndpointResponseStatus.NotSet;
    public EndpointResponseStatus? AcceptRegistrationResult { get; set; } = EndpointResponseStatus.NotSet;
    public string? OrganisationName { get; set; }
    public RegistrationFiltersModel RegistrationFilters { get; set; }
}
