using EPR.Common.Authorization.Constants;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations;

[FeatureGate(FeatureFlags.ReprocessorExporter)]
[Authorize(Policy = PolicyConstants.RegulatorBasicPolicy)]
[Route(PagePath.ReprocessorExporterRegistrations)]
public class RegistrationsController(
    ISessionManager<JourneySession> sessionManager,
    IConfiguration configuration)
    : ReprocessorExporterBaseController(sessionManager, configuration)
{
    [HttpGet]
    [Route(PagePath.AuthorisedMaterials)]
    public async Task<IActionResult> AuthorisedMaterials(int registrationTaskId)
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.AuthorisedMaterials);
        SetBackLinkInfos(session, PagePath.AuthorisedMaterials);
        
        return View(GetRegistrationsView(nameof(AuthorisedMaterials)));
    }

    [HttpGet]
    [Route(PagePath.UkSiteDetails)]
    public async Task<IActionResult> UkSiteDetails(int registrationTaskId)
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.UkSiteDetails);

        SetBackLinkInfos(session, PagePath.UkSiteDetails);

        return View(GetRegistrationsView(nameof(UkSiteDetails)));
    }

    [HttpGet]
    [Route(PagePath.MaterialWasteLicences)]
    public async Task<IActionResult> MaterialWasteLicences()
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.MaterialWasteLicences);
        SetBackLinkInfos(session, PagePath.MaterialWasteLicences);

        return View(GetRegistrationsView(nameof(MaterialWasteLicences)));
    }

    [HttpGet]
    [Route(PagePath.SamplingInspection)]
    public async Task<IActionResult> SamplingInspection()
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.SamplingInspection);
        SetBackLinkInfos(session, PagePath.SamplingInspection);

        return View(GetRegistrationsView(nameof(SamplingInspection)));
    }
    
    [HttpGet]
    [Route(PagePath.InputsAndOutputs)]
    public async Task<IActionResult> InputsAndOutputs()
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.InputsAndOutputs);
        SetBackLinkInfos(session, PagePath.InputsAndOutputs);

        return View(GetRegistrationsView(nameof(InputsAndOutputs)));
    }

    [HttpGet]
    [Route(PagePath.WasteLicences)]
    public async Task<IActionResult> WasteLicences(int registrationTaskId)
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.WasteLicences);
        SetBackLinkInfos(session, PagePath.WasteLicences);

        return View(GetRegistrationsView(nameof(WasteLicences)));
    }

    [HttpGet]
    [Route(PagePath.BusinessAddress)]
    public async Task<IActionResult> BusinessAddress(int registrationTaskId)
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.BusinessAddress);
        SetBackLinkInfos(session, PagePath.BusinessAddress);

        return View(GetRegistrationsView(nameof(BusinessAddress)));
    }

    [HttpGet]
    [Route(PagePath.MaterialDetails)]
    public async Task<IActionResult> MaterialDetails()
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.MaterialDetails);
        SetBackLinkInfos(session, PagePath.MaterialDetails);

        return View(GetRegistrationsView(nameof(MaterialDetails)));
    }

    [HttpGet]
    [Route(PagePath.OverseasReprocessorInterim)]
    public async Task<IActionResult> OverseasReprocessorInterim()
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.OverseasReprocessorInterim);
        SetBackLinkInfos(session, PagePath.OverseasReprocessorInterim);

        return View(GetRegistrationsView(nameof(OverseasReprocessorInterim)));
    }
}