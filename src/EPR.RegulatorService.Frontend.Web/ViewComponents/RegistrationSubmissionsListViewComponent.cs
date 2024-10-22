

using System.Diagnostics.CodeAnalysis;
using System.Linq;

using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Web.ViewModels.Shared;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace EPR.RegulatorService.Frontend.Web.ViewComponents;

public class RegistrationSubmissionsListViewComponent(IFacadeService facadeService, IHttpContextAccessor httpContextAccessor) : ViewComponent
{
    [ExcludeFromCodeCoverage]
    public async Task<ViewViewComponentResult> InvokeAsync(RegistrationSubmissionsListRequest request)
    {
        var pagedOrganisationRegistrations = await facadeService.GetRegistrationSubmissions(new RegistrationSubmissionsFilterModel { Page = request.PageNumber });

        if ((request.PageNumber > pagedOrganisationRegistrations.TotalPages && request.PageNumber > 1) || request.PageNumber < 1)
        {
            httpContextAccessor.HttpContext.Response.Redirect(PagePath.PageNotFoundPath);
        }

        var model = new RegistrationSubmissionsListViewModel
        {
            PagedRegistrationSubmissions = pagedOrganisationRegistrations.Items.Select(x=>(RegistrationSubmissionDetailsViewModel)x),
            PaginationNavigationModel = new PaginationNavigationModel
            {
                CurrentPage = pagedOrganisationRegistrations.CurrentPage,
                PageCount = pagedOrganisationRegistrations.TotalPages,
                ControllerName = "RegistrationSubmissions",
                ActionName = nameof(RegistrationSubmissionsController.RegistrationSubmissions)
            }
        };

        return View(model);
    }
}