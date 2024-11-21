using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EPR;
using EPR.RegulatorService;
using EPR.RegulatorService.Frontend;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web;
using EPR.RegulatorService.Frontend.Web.Controllers.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewComponents;
using EPR.RegulatorService.Frontend.Web.ViewComponents.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Web.ViewModels.Shared;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace EPR.RegulatorService.Frontend.Web.ViewComponents.RegistrationSubmissions;

public class RegistrationSubmissionListViewComponent(IFacadeService facadeService, IHttpContextAccessor httpContextAccessor, ISessionManager<JourneySession> sessionManager) : ViewComponent
{
    [ExcludeFromCodeCoverage]
    public async Task<ViewViewComponentResult> InvokeAsync(RegistrationSubmissionsListViewModel request)
    {
        var pagedOrganisationRegistrations = await facadeService.GetRegistrationSubmissions(request.RegistrationsFilterModel);

        var currentSession = await sessionManager.GetSessionAsync(httpContextAccessor.HttpContext.Session);

        request.PagedRegistrationSubmissions = currentSession.RegulatorRegistrationSubmissionSession.OrganisationDetailsChangeHistory.Count > 0
                                               ? pagedOrganisationRegistrations.Items
                                                    .Select(x => currentSession.RegulatorRegistrationSubmissionSession.OrganisationDetailsChangeHistory.TryGetValue(x.SubmissionId, out var rs)
                                               ? (RegistrationSubmissionDetailsViewModel)rs : (RegistrationSubmissionDetailsViewModel)x)
                                               : pagedOrganisationRegistrations.Items.Select(x => (RegistrationSubmissionDetailsViewModel)x);

        request.PaginationNavigationModel = new PaginationNavigationModel
        {
            CurrentPage = pagedOrganisationRegistrations.CurrentPage,
            PageCount = pagedOrganisationRegistrations.TotalPages,
            ControllerName = "RegistrationSubmissions",
            ActionName = nameof(RegistrationSubmissionsController.RegistrationSubmissions)
        };

        if (request.PaginationNavigationModel.CurrentPage > pagedOrganisationRegistrations.TotalPages &&
            request.PaginationNavigationModel.CurrentPage > 1 || request.PaginationNavigationModel.CurrentPage < 1)
        {
            request.PaginationNavigationModel.CurrentPage = 1;
        }

        return View(request);
    }
}