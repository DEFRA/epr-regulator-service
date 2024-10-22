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
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EPR.RegulatorService.Frontend.Web.ViewComponents;

public class RegistrationSubmissionListViewComponent(IFacadeService facadeService, IHttpContextAccessor httpContextAccessor) : ViewComponent
{
    [ExcludeFromCodeCoverage]
    public async Task<ViewViewComponentResult> InvokeAsync(RegistrationSubmissionsListViewModel request)
    {
        var pagedOrganisationRegistrations = await facadeService.GetRegistrationSubmissions(request.CreateFilterModel(request.PaginationNavigationModel.CurrentPage));

        if ((request.PaginationNavigationModel.CurrentPage > pagedOrganisationRegistrations.TotalPages && request.PaginationNavigationModel.CurrentPage > 1) || request.PaginationNavigationModel.CurrentPage < 1)
        {
            httpContextAccessor.HttpContext.Response.Redirect(PagePath.PageNotFoundPath);
        }

        request.PagedRegistrationSubmissions = pagedOrganisationRegistrations.Items.Select(x => (RegistrationSubmissionDetailsViewModel)x);
        request.PaginationNavigationModel = new PaginationNavigationModel
        {
            CurrentPage = pagedOrganisationRegistrations.CurrentPage,
            PageCount = pagedOrganisationRegistrations.TotalPages,
            ControllerName = "RegistrationSubmissions",
            ActionName = nameof(RegistrationSubmissionsController.RegistrationSubmissions)
        };

        return View(request);
    }
}