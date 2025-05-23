using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Web.Controllers.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Web.ViewModels.Shared;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace EPR.RegulatorService.Frontend.Web.ViewComponents;

public class RegistrationSubmissionListViewComponent(IFacadeService facadeService) : ViewComponent
{
    [ExcludeFromCodeCoverage]
    public async Task<ViewViewComponentResult> InvokeAsync(RegistrationSubmissionsListViewModel request)
    {
        var pagedOrganisationRegistrations = await facadeService.GetRegistrationSubmissions(request.RegistrationsFilterModel);

        request.PagedRegistrationSubmissions = pagedOrganisationRegistrations.items.Select(x => (RegistrationSubmissionDetailsViewModel)x);
        request.PaginationNavigationModel = new PaginationNavigationModel
        {
            CurrentPage = pagedOrganisationRegistrations.currentPage,
            PageCount = pagedOrganisationRegistrations.TotalPages,
            ControllerName = "RegistrationSubmissions",
            ActionName = nameof(RegistrationSubmissionsController.RegistrationSubmissions)
        };

        if ((request.PaginationNavigationModel.CurrentPage > pagedOrganisationRegistrations.TotalPages &&
            request.PaginationNavigationModel.CurrentPage > 1) || request.PaginationNavigationModel.CurrentPage < 1)
        {
            request.PaginationNavigationModel.CurrentPage = 1;
        }

        return View(request);
    }
}