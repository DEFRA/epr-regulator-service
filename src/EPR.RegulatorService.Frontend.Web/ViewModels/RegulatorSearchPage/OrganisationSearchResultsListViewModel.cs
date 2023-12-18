using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Web.ViewModels.Shared;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegulatorSearchPage;

public class OrganisationSearchResultsListViewModel
{
    public PaginationNavigationModel PaginationNavigationModel { get; set; }

    public IEnumerable<OrganisationSearchResult>? PagedOrganisationSearchResults { get; set; }

    public OrganisationSearchFilterModel OrganisationSearchFilterModel { get; set; }
}