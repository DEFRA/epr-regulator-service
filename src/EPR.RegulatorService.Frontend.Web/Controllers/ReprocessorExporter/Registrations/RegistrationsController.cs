using EPR.Common.Authorization.Constants;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations;

using Core.Enums.ReprocessorExporter;

using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;

[FeatureGate(FeatureFlags.ReprocessorExporter)]
[Authorize(Policy = PolicyConstants.RegulatorBasicPolicy)]
[Route(PagePath.ReprocessorExporterRegistrations)]
public class RegistrationsController(
    ISessionManager<JourneySession> sessionManager,
    IReprocessorExporterService reprocessorExporterService,
    IConfiguration configuration)
    : ReprocessorExporterBaseController(sessionManager, configuration)
{
    [HttpGet]
    [Route(PagePath.AuthorisedMaterials)]
    public async Task<IActionResult> AuthorisedMaterials(int registrationId)
    {
        // TaskName = RegulatorTaskType.MaterialsAuthorisedOnSite
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.AuthorisedMaterials);
        SetBackLinkInfos(session, PagePath.AuthorisedMaterials);
        
        return View(GetRegistrationsView(nameof(AuthorisedMaterials)), registrationId);
    }

    [HttpGet]
    [Route(PagePath.CompleteAuthorisedMaterials)]
    public async Task<IActionResult> CompleteAuthorisedMaterials(int registrationId)
    {        
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.CompleteAuthorisedMaterials);
        SetBackLinkInfos(session, PagePath.CompleteAuthorisedMaterials);

        var updateRegistrationTaskStatusRequest = new UpdateRegistrationTaskStatusRequest
        {
            TaskName = (int)RegulatorTaskType.MaterialsAuthorisedOnSite,
            RegistrationId = registrationId,
            Status = "Complete",
        };

        await reprocessorExporterService.UpdateRegulatorRegistrationTaskStatusAsync(updateRegistrationTaskStatusRequest);

        return RedirectToAction("Index", "ManageRegistrations", new { id = registrationId });
    }

    [HttpGet]
    [Route(PagePath.UkSiteDetails)]
    public async Task<IActionResult> UkSiteDetails(int registrationId)
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.UkSiteDetails);

        SetBackLinkInfos(session, PagePath.UkSiteDetails);

        return View(GetRegistrationsView(nameof(UkSiteDetails)), registrationId);
    }

    [HttpGet]
    [Route(PagePath.CompleteUkSiteDetails)]
    public async Task<IActionResult> CompleteUkSiteDetails(int registrationId)
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.UkSiteDetails);

        SetBackLinkInfos(session, PagePath.UkSiteDetails);

        var updateRegistrationTaskStatusRequest = new UpdateRegistrationTaskStatusRequest
        {
            TaskName = (int)RegulatorTaskType.SiteAddressAndContactDetails,
            RegistrationId = registrationId,
            Status = "Complete",
        };

        await reprocessorExporterService.UpdateRegulatorRegistrationTaskStatusAsync(updateRegistrationTaskStatusRequest);

        return RedirectToAction("Index", "ManageRegistrations", new { id = registrationId });
    }

    [HttpGet]
    [Route(PagePath.MaterialWasteLicences)]
    public async Task<IActionResult> MaterialWasteLicences(int registrationMaterialId)
    {
        //TaskName = RegulatorTaskType.WasteLicensesPermitsAndExemptions
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.MaterialWasteLicences);
        SetBackLinkInfos(session, PagePath.MaterialWasteLicences);

        return View(GetRegistrationsView(nameof(MaterialWasteLicences)));
    }

    [HttpGet]
    [Route(PagePath.CompleteMaterialWasteLicences)]
    public async Task<IActionResult> CompleteMaterialWasteLicences(int registrationMaterialId)
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.CompleteMaterialWasteLicences);
        SetBackLinkInfos(session, PagePath.CompleteMaterialWasteLicences);

        var updateRegistrationTaskStatusRequest = new UpdateMaterialTaskStatusRequest
        {
            TaskName = (int)RegulatorTaskType.WasteLicensesPermitsAndExemptions,
            RegistrationMaterialId = registrationMaterialId,
            Status = "Complete",
        };

        await reprocessorExporterService.UpdateRegulatorApplicationTaskStatusAsync(updateRegistrationTaskStatusRequest);

        return RedirectToAction("Index", "ManageRegistrations", new { id = registrationMaterialId });
    }

    [HttpGet]
    [Route(PagePath.SamplingInspection)]
    public async Task<IActionResult> SamplingInspection(int registrationMaterialId)
    {
        //TaskName = RegulatorTaskType.SamplingAndInspectionPlan
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.SamplingInspection);
        SetBackLinkInfos(session, PagePath.SamplingInspection);

        return View(GetRegistrationsView(nameof(SamplingInspection)));
    }
    
    [HttpGet]
    [Route(PagePath.InputsAndOutputs)]
    public async Task<IActionResult> InputsAndOutputs(int registrationMaterialId)
    {
        //TaskName = RegulatorTaskType.ReprocessingInputsAndOutputs
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.InputsAndOutputs);
        SetBackLinkInfos(session, PagePath.InputsAndOutputs);

        return View(GetRegistrationsView(nameof(InputsAndOutputs)));
    }

    [HttpGet]
    [Route(PagePath.WasteLicences)]
    public async Task<IActionResult> WasteLicences(int registrationId)
    {
        //TaskName = RegulatorTaskType.WasteLicensesPermitsAndExemptions
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.WasteLicences);
        SetBackLinkInfos(session, PagePath.WasteLicences);

        return View(GetRegistrationsView(nameof(WasteLicences)));
    }

    [HttpGet]
    [Route(PagePath.BusinessAddress)]
    public async Task<IActionResult> BusinessAddress(int registrationId)
    {
        //TaskName = RegulatorTaskType.BusinessAddress
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.BusinessAddress);
        SetBackLinkInfos(session, PagePath.BusinessAddress);

        return View(GetRegistrationsView(nameof(BusinessAddress)), registrationId);
    }

    [HttpGet]
    [Route(PagePath.CompleteBusinessAddress)]
    public async Task<IActionResult> CompleteBusinessAddress(int registrationId)
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.BusinessAddress);
        SetBackLinkInfos(session, PagePath.BusinessAddress);
        var updateRegistrationTaskStatusRequest = new UpdateRegistrationTaskStatusRequest
        {
            TaskName = (int)RegulatorTaskType.BusinessAddress,
            RegistrationId = registrationId,
            Status = "Complete",
        };

        await reprocessorExporterService.UpdateRegulatorRegistrationTaskStatusAsync(updateRegistrationTaskStatusRequest);

        return RedirectToAction("Index", "ManageRegistrations", new { id = registrationId });
    }

    [HttpGet]
    [Route(PagePath.MaterialDetails)]
    public async Task<IActionResult> MaterialDetails(int registrationMaterialId)
    {
        //TaskName = RegulatorTaskType.MaterialDetailsAndContact
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.MaterialDetails);
        SetBackLinkInfos(session, PagePath.MaterialDetails);

        return View(GetRegistrationsView(nameof(MaterialDetails)));
    }

    [HttpGet]
    [Route(PagePath.OverseasReprocessorInterim)]
    public async Task<IActionResult> OverseasReprocessorInterim(int registrationMaterialId)
    {
        //TaskName = RegulatorTaskType.OverseasReprocessorAndInterimSiteDetails
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.OverseasReprocessorInterim);
        SetBackLinkInfos(session, PagePath.OverseasReprocessorInterim);

        return View(GetRegistrationsView(nameof(OverseasReprocessorInterim)));
    }
}