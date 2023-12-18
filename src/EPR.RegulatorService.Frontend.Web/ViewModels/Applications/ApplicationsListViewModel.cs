using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Web.ViewModels.Shared;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.Applications
{
    public class ApplicationsListViewModel
    {
        public IEnumerable<OrganisationApplications>? PagedOrganisationApplications { get; set; }
        
        public PaginationNavigationModel PaginationNavigationModel { get; set; }
        
        public RegulatorApplicationFiltersModel RegulatorApplicationFiltersModel { get; set; }
    }
}