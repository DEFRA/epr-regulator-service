using Microsoft.AspNetCore.Mvc;
using EPR.RegulatorService.Frontend.Core.Services;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using EPR.RegulatorService.Frontend.Web.ViewModels.Applications;

namespace EPR.RegulatorService.Frontend.Web.ViewComponents;

public class ApplicationsListViewComponent : ViewComponent
{
    private const string ApprovedPerson = "ApprovedPerson";
    private const string DelegatedPerson = "DelegatedPerson";
    private readonly IFacadeService _facadeService;

    public ApplicationsListViewComponent(
        IFacadeService facadeService)
    {
        _facadeService = facadeService;
    }
    
    public async Task<ViewViewComponentResult> InvokeAsync(
        string? searchOrganisationName, 
        bool approvedPersonFilterSelected,
        bool delegatedPersonFilterSelected, 
        int pageNumber = 1)
    {
        var applicationType =
            SetFilteredApplicationType(approvedPersonFilterSelected, delegatedPersonFilterSelected);
        
        var pagedOrganisationApplications 
            = await _facadeService.GetUserApplicationsByOrganisation(applicationType, searchOrganisationName, pageNumber);
        
        var model = new ApplicationsListViewModel
        {
            PagedOrganisationApplications = pagedOrganisationApplications.Items,
            PaginationNavigationModel = new()
            {
                CurrentPage = pagedOrganisationApplications.CurrentPage,
                PageCount = pagedOrganisationApplications.TotalPages,
                ControllerName = "Applications",
                ActionName = "Applications"
            },
            RegulatorApplicationFiltersModel = new()
            {
                SearchOrganisationName = searchOrganisationName,
                IsApprovedUserTypeChecked = approvedPersonFilterSelected,
                IsDelegatedUserTypeChecked = delegatedPersonFilterSelected
            }
        };

        return View(model);
    }
    
    private static string SetFilteredApplicationType(bool approvedPersonFilterSelected, bool delegatedPersonFilterSelected)
    {
        if (approvedPersonFilterSelected && !delegatedPersonFilterSelected)
        {
            return ApprovedPerson;
        }

        if (delegatedPersonFilterSelected && !approvedPersonFilterSelected)
        {
            return DelegatedPerson;
        }
            
        return string.Empty;
    }
}
