using EPR.Common.Authorization.Constants;
using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

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
    public async Task<IActionResult> AuthorisedMaterials()
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.AuthorisedMaterials);
        SetBackLinkInfos(session, PagePath.AuthorisedMaterials);

        var model = new ManageRegistrationsViewModel
        {
            ApplicationOrganisationType = ApplicationOrganisationType.Reprocessor
        };
        return View(GetRegistrationsView(nameof(AuthorisedMaterials)), model);
    }

    [HttpGet]
    [Route(PagePath.UkSiteDetails)]
    public async Task<IActionResult> UkSiteDetails()
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.UkSiteDetails);

        SetBackLinkInfos(session, PagePath.UkSiteDetails);

        var model = new ManageRegistrationsViewModel
        {
            ApplicationOrganisationType = ApplicationOrganisationType.Reprocessor
        };

        return View(GetRegistrationsView(nameof(UkSiteDetails)), model);
    }

    [HttpGet]
    [Route(PagePath.MaterialWasteLicences)]
    public async Task<IActionResult> MaterialWasteLicences()
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.MaterialWasteLicences);
        SetBackLinkInfos(session, PagePath.MaterialWasteLicences);

        var model = new ManageRegistrationsViewModel
        {
            ApplicationOrganisationType = ApplicationOrganisationType.Reprocessor
        };

        return View(GetRegistrationsView(nameof(MaterialWasteLicences)), model);
    }

    [HttpGet]
    [Route(PagePath.SamplingInspection)]
    public async Task<IActionResult> SamplingInspection()
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.SamplingInspection);
        SetBackLinkInfos(session, PagePath.SamplingInspection);

        var model = new ManageRegistrationsViewModel
        {
            ApplicationOrganisationType = ApplicationOrganisationType.Reprocessor
        };

        return View(GetRegistrationsView(nameof(SamplingInspection)), model);
    }
    
    [HttpGet]
    [Route(PagePath.InputsAndOutputs)]
    public async Task<IActionResult> InputsAndOutputs()
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.InputsAndOutputs);
        SetBackLinkInfos(session, PagePath.InputsAndOutputs);

        var model = new ManageRegistrationsViewModel
        {
            ApplicationOrganisationType = ApplicationOrganisationType.Reprocessor
        };

        return View(GetRegistrationsView(nameof(InputsAndOutputs)), model);
    }

    [HttpGet]
    [Route(PagePath.WasteLicences)]
    public async Task<IActionResult> WasteLicences()
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.WasteLicences);
        SetBackLinkInfos(session, PagePath.WasteLicences);

        var model = new ManageRegistrationsViewModel
        {
            ApplicationOrganisationType = ApplicationOrganisationType.Exporter
        };

        return View(GetRegistrationsView(nameof(WasteLicences)), model);
    }

    [HttpGet]
    [Route(PagePath.BusinessAddress)]
    public async Task<IActionResult> BusinessAddress()
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.BusinessAddress);
        SetBackLinkInfos(session, PagePath.BusinessAddress);

        var model = new ManageRegistrationsViewModel
        {
            ApplicationOrganisationType = ApplicationOrganisationType.Exporter
        };

        return View(GetRegistrationsView(nameof(BusinessAddress)), model);
    }

    [HttpGet]
    [Route(PagePath.MaterialDetails)]
    public async Task<IActionResult> MaterialDetails()
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.MaterialDetails);
        SetBackLinkInfos(session, PagePath.MaterialDetails);

        var model = new ManageRegistrationsViewModel
        {
            ApplicationOrganisationType = ApplicationOrganisationType.Exporter
        };

        return View(GetRegistrationsView(nameof(MaterialDetails)), model);
    }

    [HttpGet]
    [Route(PagePath.OverseasReprocessorInterim)]
    public async Task<IActionResult> OverseasReprocessorInterim()
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.OverseasReprocessorInterim);
        SetBackLinkInfos(session, PagePath.OverseasReprocessorInterim);

        var model = new ManageRegistrationsViewModel
        {
            ApplicationOrganisationType = ApplicationOrganisationType.Exporter
        };

        return View(GetRegistrationsView(nameof(OverseasReprocessorInterim)), model);
    }
}