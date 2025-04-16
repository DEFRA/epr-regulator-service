using EPR.Common.Authorization.Constants;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations;

using System.Globalization;

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
            TaskName = RegulatorTaskType.MaterialsAuthorisedOnSite.ToString(),
            RegistrationId = registrationId,
            TaskStatus = RegulatorTaskStatus.Completed.ToString(),
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
            TaskName = RegulatorTaskType.SiteAddressAndContactDetails.ToString(),
            RegistrationId = registrationId,
            TaskStatus = RegulatorTaskStatus.Completed.ToString(),
        };

        await reprocessorExporterService.UpdateRegulatorRegistrationTaskStatusAsync(updateRegistrationTaskStatusRequest);

        return RedirectToAction("Index", "ManageRegistrations", new { id = registrationId });
    }

    [HttpGet]
    [Route(PagePath.MaterialWasteLicences)]
    public async Task<IActionResult> MaterialWasteLicences(int registrationMaterialId)
    {       
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
            TaskName = RegulatorTaskType.WasteLicensesPermitsAndExemptions.ToString(),
            RegistrationMaterialId = registrationMaterialId,
            TaskStatus = RegulatorTaskStatus.Completed.ToString(),
        };

        await reprocessorExporterService.UpdateRegulatorApplicationTaskStatusAsync(updateRegistrationTaskStatusRequest);

        int registrationId = GetRegistrationIdFromSession(session);

        return RedirectToAction("Index", "ManageRegistrations", new { id = registrationId });
    }

    [HttpGet]
    [Route(PagePath.SamplingInspection)]
    public async Task<IActionResult> SamplingInspection(int registrationMaterialId)
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.SamplingInspection);
        SetBackLinkInfos(session, PagePath.SamplingInspection);

        return View(GetRegistrationsView(nameof(SamplingInspection)));
    }

    [HttpGet]
    [Route(PagePath.CompleteSamplingInspection)]
    public async Task<IActionResult> CompleteSamplingInspection(int registrationMaterialId)
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.SamplingInspection);
        SetBackLinkInfos(session, PagePath.SamplingInspection);

        var updateRegistrationTaskStatusRequest = new UpdateMaterialTaskStatusRequest
        {
            TaskName = RegulatorTaskType.SamplingAndInspectionPlan.ToString(),
            RegistrationMaterialId = registrationMaterialId,
            TaskStatus = RegulatorTaskStatus.Completed.ToString(),
        };

        await reprocessorExporterService.UpdateRegulatorApplicationTaskStatusAsync(updateRegistrationTaskStatusRequest);

        int registrationId = GetRegistrationIdFromSession(session);

        return RedirectToAction("Index", "ManageRegistrations", new { id = registrationId });
    }

    [HttpGet]
    [Route(PagePath.InputsAndOutputs)]
    public async Task<IActionResult> InputsAndOutputs(int registrationMaterialId)
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.InputsAndOutputs);
        SetBackLinkInfos(session, PagePath.InputsAndOutputs);

        return View(GetRegistrationsView(nameof(InputsAndOutputs)));     
    }

    [HttpGet]
    [Route(PagePath.CompleteInputsAndOutputs)]
    public async Task<IActionResult> CompleteInputsAndOutputs(int registrationMaterialId)
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.CompleteInputsAndOutputs);
        SetBackLinkInfos(session, PagePath.CompleteInputsAndOutputs);

        var updateRegistrationTaskStatusRequest = new UpdateMaterialTaskStatusRequest
        {
            TaskName = RegulatorTaskType.ReprocessingInputsAndOutputs.ToString(),
            RegistrationMaterialId = registrationMaterialId,
            TaskStatus = RegulatorTaskStatus.Completed.ToString(),
        };

        await reprocessorExporterService.UpdateRegulatorApplicationTaskStatusAsync(updateRegistrationTaskStatusRequest);

        int registrationId = GetRegistrationIdFromSession(session);

        return RedirectToAction("Index", "ManageRegistrations", new { id = registrationId });
    }

    [HttpGet]
    [Route(PagePath.WasteLicences)]
    public async Task<IActionResult> WasteLicences(int registrationId)
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.WasteLicences);
        SetBackLinkInfos(session, PagePath.WasteLicences);

        return View(GetRegistrationsView(nameof(WasteLicences)));
    }

    [HttpGet]
    [Route(PagePath.CompleteWasteLicences)]
    public async Task<IActionResult> CompleteWasteLicences(int registrationId)
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.WasteLicences);
        SetBackLinkInfos(session, PagePath.WasteLicences);

        var updateRegistrationTaskStatusRequest = new UpdateRegistrationTaskStatusRequest
        {
            TaskName = RegulatorTaskType.WasteLicensesPermitsAndExemptions.ToString(),
            RegistrationId = registrationId,
            TaskStatus = RegulatorTaskStatus.Completed.ToString(),
        };

        await reprocessorExporterService.UpdateRegulatorRegistrationTaskStatusAsync(updateRegistrationTaskStatusRequest);

        return RedirectToAction("Index", "ManageRegistrations", new { id = registrationId });
    }

    [HttpGet]
    [Route(PagePath.BusinessAddress)]
    public async Task<IActionResult> BusinessAddress(int registrationId)
    {        
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

        await SaveSessionAndJourney(session, PagePath.CompleteBusinessAddress);
        SetBackLinkInfos(session, PagePath.CompleteBusinessAddress);
        var updateRegistrationTaskStatusRequest = new UpdateRegistrationTaskStatusRequest
        {
            TaskName = RegulatorTaskType.BusinessAddress.ToString(),
            RegistrationId = registrationId,
            TaskStatus = RegulatorTaskStatus.Completed.ToString(),
        };

        await reprocessorExporterService.UpdateRegulatorRegistrationTaskStatusAsync(updateRegistrationTaskStatusRequest);

        return RedirectToAction("Index", "ManageRegistrations", new { id = registrationId });
    }

    [HttpGet]
    [Route(PagePath.MaterialDetails)]
    public async Task<IActionResult> MaterialDetails(int registrationMaterialId)
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.MaterialDetails);
        SetBackLinkInfos(session, PagePath.MaterialDetails);

        return View(GetRegistrationsView(nameof(MaterialDetails)));
    }

    [HttpGet]
    [Route(PagePath.CompleteMaterialDetails)]
    public async Task<IActionResult> CompleteMaterialDetails(int registrationMaterialId)
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.MaterialDetails);
        SetBackLinkInfos(session, PagePath.MaterialDetails);

        var updateRegistrationTaskStatusRequest = new UpdateMaterialTaskStatusRequest
        {
            TaskName = RegulatorTaskType.MaterialDetailsAndContact.ToString(),
            RegistrationMaterialId = registrationMaterialId,
            TaskStatus = RegulatorTaskStatus.Completed.ToString(),
        };

        await reprocessorExporterService.UpdateRegulatorApplicationTaskStatusAsync(updateRegistrationTaskStatusRequest);

        int registrationId = GetRegistrationIdFromSession(session);

        return RedirectToAction("Index", "ManageRegistrations", new { id = registrationId });
    }

    [HttpGet]
    [Route(PagePath.OverseasReprocessorInterim)]
    public async Task<IActionResult> OverseasReprocessorInterim(int registrationMaterialId)
    {        
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.OverseasReprocessorInterim);
        SetBackLinkInfos(session, PagePath.OverseasReprocessorInterim);

        return View(GetRegistrationsView(nameof(OverseasReprocessorInterim)));
    }

    [HttpGet]
    [Route(PagePath.CompleteOverseasReprocessorInterim)]
    public async Task<IActionResult> CompleteOverseasReprocessorInterim(int registrationMaterialId)
    {        
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.OverseasReprocessorInterim);
        SetBackLinkInfos(session, PagePath.OverseasReprocessorInterim);

        var updateRegistrationTaskStatusRequest = new UpdateMaterialTaskStatusRequest
        {
            TaskName = RegulatorTaskType.OverseasReprocessorAndInterimSiteDetails.ToString(),
            RegistrationMaterialId = registrationMaterialId,
            TaskStatus = RegulatorTaskStatus.Completed.ToString(),
        };

        await reprocessorExporterService.UpdateRegulatorApplicationTaskStatusAsync(updateRegistrationTaskStatusRequest);

        int registrationId = GetRegistrationIdFromSession(session);

        return RedirectToAction("Index", "ManageRegistrations", new { id = registrationId });
    }

    private int GetRegistrationIdFromSession(JourneySession session)
    {
        string lastJourney = session.RegulatorSession.Journey.LastOrDefault(x => x.StartsWith("manage-registrations?id=", StringComparison.Ordinal));
        if (lastJourney == null)
        {
            // Handle the case where lastJourney is null
            throw new InvalidOperationException("Cannot find registrationId from ");
        }

        int registrationId = Convert.ToInt32(lastJourney.Split('=')[1], CultureInfo.InvariantCulture);
        return registrationId;
    }
}