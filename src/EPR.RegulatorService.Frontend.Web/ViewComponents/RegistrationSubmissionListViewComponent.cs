

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Schema;

using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Models.Registrations;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.Registrations;
using EPR.RegulatorService.Frontend.Web.Controllers.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Web.ViewModels.Registrations;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Web.ViewModels.Shared;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace EPR.RegulatorService.Frontend.Web.ViewComponents;

public class RegistrationSubmissionListViewComponent(IFacadeService facadeService, IHttpContextAccessor httpContextAccessor) : ViewComponent
{
    [ExcludeFromCodeCoverage]
    public async Task<ViewViewComponentResult> InvokeAsync(RegistrationSubmissionsListRequest request)
    {
        // to do: set filters here

        var pagedOrganisationRegistrations
            = await facadeService.GetRegistrationSubmissions(request.PageNumber);

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