using System.Net;
using EPR.Common.Authorization.Constants;
using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Extensions;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.Errors;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations;

[FeatureGate(FeatureFlags.ReprocessorExporter)]
[Authorize(Policy = PolicyConstants.RegulatorBasicPolicy)]
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
    [Route(PagePath.SamplingInspection)]
    public async Task<IActionResult> SamplingInspection()
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.ManageRegistrations, PagePath.SamplingInspection);
        SetBackLinkInfos(session, PagePath.SamplingInspection);

        var model = new ManageRegistrationsViewModel
        {
            ApplicationOrganisationType = ApplicationOrganisationType.Reprocessor
        };

        return View("~/Views/ReprocessorExporter/Registrations/SamplingInspection.cshtml", model);
    }


    [HttpGet]
    [Route(PagePath.InputsAndOutputs)]
    public async Task<IActionResult> InputsAndOutputs()
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.ManageRegistrations, PagePath.InputsAndOutputs);
        SetBackLinkInfos(session, PagePath.InputsAndOutputs);

        var model = new ManageRegistrationsViewModel
        {
            ApplicationOrganisationType = ApplicationOrganisationType.Reprocessor
        };

        return View("~/Views/ReprocessorExporter/Registrations/InputsAndOutputs.cshtml", model);
    }

    [HttpGet]
    [Route(PagePath.BusinessAddress)]
    public async Task<IActionResult> BusinessAddress()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        if (session?.ReprocessorExporterSession?.Journey == null)
        {
            return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new { statusCode = (int)HttpStatusCode.InternalServerError });
        }

        session.ReprocessorExporterSession.Journey.AddIfNotExists(PagePath.ManageRegistrations);
        SaveSessionAndJourney(session, PagePath.ManageRegistrations, PagePath.BusinessAddress);
        SetBackLinkInfos(session, PagePath.BusinessAddress);

        var model = new ManageRegistrationsViewModel
        {
            ApplicationOrganisationType = ApplicationOrganisationType.Exporter
        };

        return View("~/Views/ReprocessorExporter/Registrations/BusinessAddress.cshtml", model);
    }

    [HttpGet]
    [Route(PagePath.OverseasReprocessorInterim)]
    public async Task<IActionResult> OverseasReprocessorInterim()
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.ManageRegistrations, PagePath.OverseasReprocessorInterim);
        SetBackLinkInfos(session, PagePath.OverseasReprocessorInterim);

        var model = new ManageRegistrationsViewModel
        {
            ApplicationOrganisationType = ApplicationOrganisationType.Exporter
        };

        return View("~/Views/ReprocessorExporter/Registrations/OverseasReprocessorInterim.cshtml", model);
    }

    private void SetBackLinkInfos(JourneySession session, string currentPagePath)
    {
        if (string.IsNullOrEmpty(Request?.Headers?.Referer))
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