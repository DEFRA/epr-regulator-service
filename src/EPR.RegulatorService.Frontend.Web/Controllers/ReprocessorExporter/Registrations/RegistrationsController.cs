using EPR.Common.Authorization.Constants;
using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Extensions;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations;

[FeatureGate(FeatureFlags.ReprocessorExporter)]
[Authorize(Policy = PolicyConstants.RegulatorBasicPolicy)]
[Route(PagePath.ReprocessorExporterRegistrations)]
public class RegistrationsController : RegulatorSessionBaseController
{
    public RegistrationsController(ISessionManager<JourneySession> sessionManager, IConfiguration configuration) : base(sessionManager, configuration) { }

    [HttpGet]
    [Route(PagePath.AuthorisedMaterials)]
    public async Task<IActionResult> AuthorisedMaterials()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new JourneySession();
        session.ReprocessorExporterSession.Journey.AddIfNotExists(PagePath.ManageRegistrations);
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
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new JourneySession();
        session.ReprocessorExporterSession.Journey.AddIfNotExists(PagePath.ManageRegistrations);
        await SaveSessionAndJourney(session, PagePath.ManageRegistrations, PagePath.UkSiteDetails);
        SetBackLinkInfos(session, PagePath.UkSiteDetails);
        var model = new ManageRegistrationsViewModel
        {
            ApplicationOrganisationType = ApplicationOrganisationType.Reprocessor
        };
        return View("~/Views/ReprocessorExporter/Registrations/UkSiteDetails.cshtml", model);
    }

    [HttpGet]
    [Route(PagePath.SamplingInspection)]
    public async Task<IActionResult> SamplingInspection()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new JourneySession();
        session.ReprocessorExporterSession.Journey.AddIfNotExists(PagePath.ManageRegistrations);
        SaveSessionAndJourney(session, PagePath.ManageRegistrations, PagePath.SamplingInspection);
        SetBackLinkInfos(session, PagePath.SamplingInspection);
        var model = new ManageRegistrationsViewModel
        {
            ApplicationOrganisationType = ApplicationOrganisationType.Reprocessor
        };

        return View("~/Views/ReprocessorExporter/Registrations/SamplingInspection.cshtml", model);
    }

    private void SetBackLinkInfos(JourneySession session, string currentPagePath)
    {
        if (string.IsNullOrEmpty(Request.Headers.Referer))
            SetHomeBackLink();
        else
            SetBackLink(session, currentPagePath);

        SetBackLinkAriaLabel();
    }
}