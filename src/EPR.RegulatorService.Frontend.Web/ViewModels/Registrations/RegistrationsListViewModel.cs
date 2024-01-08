using EPR.RegulatorService.Frontend.Web.ViewModels.Shared;
using EPR.RegulatorService.Frontend.Core.Models.Registrations;
using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.Registrations
{
    [ExcludeFromCodeCoverage]
    public class RegistrationsListViewModel
    {
        public IEnumerable<Registration>? PagedOrganisationRegistrations { get; set; }
        public PaginationNavigationModel PaginationNavigationModel { get; set; }
        public RegulatorRegistrationFiltersModel RegulatorRegistrationFiltersModel { get; set; }
    }
}
