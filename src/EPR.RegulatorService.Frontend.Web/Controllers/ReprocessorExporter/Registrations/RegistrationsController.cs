using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Extensions;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations;

[FeatureGate(FeatureFlags.ReprocessorExporter)]
[Route(PagePath.ReprocessorExporterRegistrations)]
public class RegistrationsController(ISessionManager<JourneySession> sessionManager, IConfiguration configuration)
    : RegulatorSessionBaseController(sessionManager, configuration)
{
    [HttpGet]
    [Route(PagePath.AuthorisedMaterials)]
    public async Task<IActionResult> AuthorisedMaterials()
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.ManageRegistrations, PagePath.AuthorisedMaterials);
        SetBackLinkInfos(session, PagePath.AuthorisedMaterials);

        var model = new ManageRegistrationsViewModel
        {
            ApplicationOrganisationType = ApplicationOrganisationType.Reprocessor
        };

        return View("~/Views/ReprocessorExporter/Registrations/AuthorisedMaterials.cshtml", model);
    }

    [HttpGet]
    [Route(PagePath.UkSiteDetails)]
    public async Task<IActionResult> UkSiteDetails()
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.ManageRegistrations, PagePath.UkSiteDetails);
        SetBackLinkInfos(session, PagePath.UkSiteDetails);

        var model = new ManageRegistrationsViewModel
        {
            ApplicationOrganisationType = ApplicationOrganisationType.Reprocessor
        };

        return View("~/Views/ReprocessorExporter/Registrations/UkSiteDetails.cshtml", model);
    }

    [HttpGet]
    [Route(PagePath.InputsAndOutputs)]
    public async Task<IActionResult> InputsAndOutputs()
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.ManageRegistrations, PagePath.InputsAndOutputs);
        SetBackLinkInfos(session, PagePath.InputsAndOutputs);

        return View("~/Views/ReprocessorExporter/Registrations/InputsAndOutputs.cshtml");
    }

    private void SetBackLinkInfos(JourneySession session, string currentPagePath)
    {
        if (string.IsNullOrEmpty(Request.Headers.Referer))
            SetHomeBackLink();
        else
            SetBackLink(session, currentPagePath);

        SetBackLinkAriaLabel();
    }

    private async Task<JourneySession> GetSession()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new JourneySession();
        session.ReprocessorExporterSession.Journey.AddIfNotExists(PagePath.ManageRegistrations);

        return session;
    }
}