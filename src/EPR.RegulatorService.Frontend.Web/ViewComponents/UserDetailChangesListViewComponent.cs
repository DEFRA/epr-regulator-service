using Microsoft.AspNetCore.Mvc;
using EPR.RegulatorService.Frontend.Core.Services;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using EPR.RegulatorService.Frontend.Web.ViewModels.UserDetailChanges;
using EPR.RegulatorService.Frontend.Web.Constants;

namespace EPR.RegulatorService.Frontend.Web.ViewComponents;

public class UserDetailChangesListViewComponent : ViewComponent
{
    private const string ApprovedPerson = "ApprovedPerson";
    private const string DelegatedPerson = "DelegatedPerson";
    private readonly IFacadeService _facadeService;

    public UserDetailChangesListViewComponent(
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
        var applicationType = SetFilteredApplicationType(approvedPersonFilterSelected, delegatedPersonFilterSelected);
        
        var pagedPendingUserRequests = await _facadeService.GetUserDetailChangeRequestsByOrganisation(applicationType, searchOrganisationName, pageNumber);
        
        var model = new UserDetailChangesListViewModel
        {
            PagedPendingUserRequests = pagedPendingUserRequests.Items,
            PaginationNavigationModel = new()
            {
                CurrentPage = pagedPendingUserRequests.CurrentPage,
                PageCount = pagedPendingUserRequests.TotalPages,
                ControllerName = "UserDetailChanges",
                ActionName = PagePath.ManageUserDetailChanges
            },
            UserDetailChangeRequestFiltersModel = new()
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
