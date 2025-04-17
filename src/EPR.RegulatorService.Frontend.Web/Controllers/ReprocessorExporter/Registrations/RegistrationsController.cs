using System.ComponentModel.DataAnnotations;

using EPR.Common.Authorization.Constants;
using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Models.Registrations;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;
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
    IReprocessorExporterService reprocessorExporterService,
    IConfiguration configuration)
    : ReprocessorExporterBaseController(sessionManager, configuration)
{
    [HttpGet]
    [Route(PagePath.AuthorisedMaterials)]
    public async Task<IActionResult> AuthorisedMaterials(int registrationId)
    {       
        var session = await GetSession();

        string pagePath = GetRegistrationMethodPath(PagePath.AuthorisedMaterials, registrationId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);
        
        return View(GetRegistrationsView(nameof(AuthorisedMaterials)), registrationId);
    }

    [HttpPost]
    [Route(PagePath.AuthorisedMaterials)]
    public async Task<IActionResult> CompleteAuthorisedMaterials(int registrationId)
    {        
        var updateRegistrationTaskStatusRequest = new UpdateRegistrationTaskStatusRequest
        {
            TaskName = RegulatorTaskType.MaterialsAuthorisedOnSite.ToString(),
            RegistrationId = registrationId,
            Status = RegulatorTaskStatus.Completed.ToString(),
        };

        await reprocessorExporterService.UpdateRegulatorRegistrationTaskStatusAsync(updateRegistrationTaskStatusRequest);

        return RedirectToAction("Index", "ManageRegistrations", new { id = registrationId });
    }

    [HttpGet]
    [Route(PagePath.UkSiteDetails)]
    public async Task<IActionResult> UkSiteDetails(int registrationId)
    {
        var session = await GetSession();

        string pagePath = GetRegistrationMethodPath(PagePath.UkSiteDetails, registrationId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        return View(GetRegistrationsView(nameof(UkSiteDetails)), registrationId);
    }

    [HttpPost]
    [Route(PagePath.UkSiteDetails)]
    public async Task<IActionResult> CompleteUkSiteDetails(int registrationId)
    {
        var updateRegistrationTaskStatusRequest = new UpdateRegistrationTaskStatusRequest
        {
            TaskName = RegulatorTaskType.SiteAddressAndContactDetails.ToString(),
            RegistrationId = registrationId,
            Status = RegulatorTaskStatus.Completed.ToString(),
        };

        await reprocessorExporterService.UpdateRegulatorRegistrationTaskStatusAsync(updateRegistrationTaskStatusRequest);

        return RedirectToAction("Index", "ManageRegistrations", new { id = registrationId });
    }

    [HttpGet]
    [Route(PagePath.MaterialWasteLicences)]
    public async Task<IActionResult> MaterialWasteLicences(int registrationMaterialId)
    {
        var session = await GetSession();

        string pagePath = GetRegistrationMaterialMethodPath(PagePath.MaterialWasteLicences, registrationMaterialId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        return View(GetRegistrationsView(nameof(MaterialWasteLicences)), registrationMaterialId);
    }

    [HttpPost]
    [Route(PagePath.MaterialWasteLicences)]
    public async Task<IActionResult> CompleteMaterialWasteLicences(int registrationMaterialId)
    {
        var updateRegistrationTaskStatusRequest = new UpdateMaterialTaskStatusRequest
        {
            TaskName = RegulatorTaskType.WasteLicensesPermitsAndExemptions.ToString(),
            RegistrationMaterialId = registrationMaterialId,
            Status = RegulatorTaskStatus.Completed.ToString(),
        };

        await reprocessorExporterService.UpdateRegulatorApplicationTaskStatusAsync(updateRegistrationTaskStatusRequest);

        var registrationMaterial = await reprocessorExporterService.GetRegistrationMaterialByIdAsync(registrationMaterialId);

        return RedirectToAction("Index", "ManageRegistrations", new { id = registrationMaterial.RegistrationId });
    }

    [HttpGet]
    [Route(PagePath.SamplingInspection)]
    public async Task<IActionResult> SamplingInspection(int registrationMaterialId)
    {
        var session = await GetSession();

        string pagePath = GetRegistrationMaterialMethodPath(PagePath.SamplingInspection, registrationMaterialId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        return View(GetRegistrationsView(nameof(SamplingInspection)), registrationMaterialId);
    }

    [HttpPost]
    [Route(PagePath.SamplingInspection)]
    public async Task<IActionResult> CompleteSamplingInspection(int registrationMaterialId)
    {
        var updateRegistrationTaskStatusRequest = new UpdateMaterialTaskStatusRequest
        {
            TaskName = RegulatorTaskType.SamplingAndInspectionPlan.ToString(),
            RegistrationMaterialId = registrationMaterialId,
            Status = RegulatorTaskStatus.Completed.ToString(),
        };

        await reprocessorExporterService.UpdateRegulatorApplicationTaskStatusAsync(updateRegistrationTaskStatusRequest);

        var registrationMaterial = await reprocessorExporterService.GetRegistrationMaterialByIdAsync(registrationMaterialId);

        return RedirectToAction("Index", "ManageRegistrations", new { id = registrationMaterial.RegistrationId });
    }

    [HttpGet]
    [Route(PagePath.InputsAndOutputs)]
    public async Task<IActionResult> InputsAndOutputs(int registrationMaterialId)
    {
        var session = await GetSession();

        string pagePath = GetRegistrationMaterialMethodPath(PagePath.InputsAndOutputs, registrationMaterialId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        return View(GetRegistrationsView(nameof(InputsAndOutputs)), registrationMaterialId);     
    }

    [HttpPost]
    [Route(PagePath.InputsAndOutputs)]
    public async Task<IActionResult> CompleteInputsAndOutputs(int registrationMaterialId)
    {
        var updateRegistrationTaskStatusRequest = new UpdateMaterialTaskStatusRequest
        {
            TaskName = RegulatorTaskType.ReprocessingInputsAndOutputs.ToString(),
            RegistrationMaterialId = registrationMaterialId,
            Status = RegulatorTaskStatus.Completed.ToString(),
        };

        await reprocessorExporterService.UpdateRegulatorApplicationTaskStatusAsync(updateRegistrationTaskStatusRequest);

        var registrationMaterial = await reprocessorExporterService.GetRegistrationMaterialByIdAsync(registrationMaterialId);

        return RedirectToAction("Index", "ManageRegistrations", new { id = registrationMaterial.RegistrationId });
    }

    [HttpGet]
    [Route(PagePath.WasteLicences)]
    public async Task<IActionResult> WasteLicences(int registrationId)
    {
        var session = await GetSession();

        string pagePath = GetRegistrationMethodPath(PagePath.WasteLicences, registrationId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        return View(GetRegistrationsView(nameof(WasteLicences)), registrationId);
    }

    [HttpPost]
    [Route(PagePath.WasteLicences)]
    public async Task<IActionResult> CompleteWasteLicences(int registrationId)
    {
        var updateRegistrationTaskStatusRequest = new UpdateRegistrationTaskStatusRequest
        {
            TaskName = RegulatorTaskType.WasteLicensesPermitsAndExemptions.ToString(),
            RegistrationId = registrationId,
            Status = RegulatorTaskStatus.Completed.ToString(),
        };

        await reprocessorExporterService.UpdateRegulatorRegistrationTaskStatusAsync(updateRegistrationTaskStatusRequest);

        return RedirectToAction("Index", "ManageRegistrations", new { id = registrationId });
    }

    [HttpGet]
    [Route(PagePath.BusinessAddress)]
    public async Task<IActionResult> BusinessAddress(int registrationId)
    {        
        var session = await GetSession();

        string pagePath = GetRegistrationMethodPath(PagePath.BusinessAddress, registrationId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        return View(GetRegistrationsView(nameof(BusinessAddress)), registrationId);
    }

    [HttpPost]
    [Route(PagePath.BusinessAddress)]
    public async Task<IActionResult> CompleteBusinessAddress(int registrationId)
    {
        var updateRegistrationTaskStatusRequest = new UpdateRegistrationTaskStatusRequest
        {
            TaskName = RegulatorTaskType.BusinessAddress.ToString(),
            RegistrationId = registrationId,
            Status = RegulatorTaskStatus.Completed.ToString(),
        };

        await reprocessorExporterService.UpdateRegulatorRegistrationTaskStatusAsync(updateRegistrationTaskStatusRequest);

        return RedirectToAction("Index", "ManageRegistrations", new { id = registrationId });
    }

    [HttpGet]
    [Route(PagePath.MaterialDetails)]
    public async Task<IActionResult> MaterialDetails(int registrationMaterialId)
    {
        var session = await GetSession();

        string pagePath = GetRegistrationMaterialMethodPath(PagePath.MaterialDetails, registrationMaterialId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        return View(GetRegistrationsView(nameof(MaterialDetails)), registrationMaterialId);
    }

    [HttpGet]
    [Route(PagePath.MaterialDetails)]
    public async Task<IActionResult> CompleteMaterialDetails(int registrationMaterialId)
    {
        var updateRegistrationTaskStatusRequest = new UpdateMaterialTaskStatusRequest
        {
            TaskName = RegulatorTaskType.MaterialDetailsAndContact.ToString(),
            RegistrationMaterialId = registrationMaterialId,
            Status = RegulatorTaskStatus.Completed.ToString(),
        };

        await reprocessorExporterService.UpdateRegulatorApplicationTaskStatusAsync(updateRegistrationTaskStatusRequest);

        var registrationMaterial = await reprocessorExporterService.GetRegistrationMaterialByIdAsync(registrationMaterialId);

        return RedirectToAction("Index", "ManageRegistrations", new { id = registrationMaterial.RegistrationId });
    }

    [HttpGet]
    [Route(PagePath.OverseasReprocessorInterim)]
    public async Task<IActionResult> OverseasReprocessorInterim(int registrationMaterialId)
    {        
        var session = await GetSession();

        string pagePath = GetRegistrationMaterialMethodPath(PagePath.OverseasReprocessorInterim, registrationMaterialId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        return View(GetRegistrationsView(nameof(OverseasReprocessorInterim)), registrationMaterialId);
    }

    [HttpGet]
    [Route(PagePath.OverseasReprocessorInterim)]
    public async Task<IActionResult> CompleteOverseasReprocessorInterim(int registrationMaterialId)
    {        
        var updateRegistrationTaskStatusRequest = new UpdateMaterialTaskStatusRequest
        {
            TaskName = RegulatorTaskType.OverseasReprocessorAndInterimSiteDetails.ToString(),
            RegistrationMaterialId = registrationMaterialId,
            Status = RegulatorTaskStatus.Completed.ToString(),
        };

        await reprocessorExporterService.UpdateRegulatorApplicationTaskStatusAsync(updateRegistrationTaskStatusRequest);

        var registrationMaterial = await reprocessorExporterService.GetRegistrationMaterialByIdAsync(registrationMaterialId);

        return RedirectToAction("Index", "ManageRegistrations", new { id = registrationMaterial.RegistrationId });
    }

    [HttpPost]
    [Route(PagePath.QueryRegistrationTask)]
    public async Task<IActionResult> QueryRegistrationTask(int registrationId, RegulatorTaskType taskName)
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.QueryRegistrationTask);
        SetBackLinkInfos(session, PagePath.QueryRegistrationTask);

        return View(GetRegistrationsView(nameof(QueryRegistrationTask)), new { registrationId, taskName } );
    }

    [HttpGet]
    [Route(PagePath.QueryMaterialTask)]
    public async Task<IActionResult> QueryMaterialTask(int registrationMaterialId, string taskName)
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.QueryMaterialTask);
        SetBackLinkInfos(session, PagePath.QueryMaterialTask);

        return View(GetRegistrationsView(nameof(QueryMaterialTask)), new { registrationMaterialId, taskName });
    }

    [HttpPost]
    [Route(PagePath.CompleteQueryRegistrationTask)]
    public async Task<IActionResult> CompleteQueryRegistrationTask(
        int registrationId,
        string taskName,
        [Required(AllowEmptyStrings = false, ErrorMessage = "Comments must be provided")]
        [MaxLength(500, ErrorMessage = "Comments must be 500 characters or less")]
        string comments)
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.CompleteQueryRegistrationTask);
        SetBackLinkInfos(session, PagePath.CompleteQueryRegistrationTask);

        var updateRegistrationTaskStatusRequest = new UpdateRegistrationTaskStatusRequest
        {
            TaskName = taskName,
            RegistrationId = registrationId,
            Status = RegulatorTaskStatus.Queried.ToString(),
            Comments = comments
        };

        await reprocessorExporterService.UpdateRegulatorRegistrationTaskStatusAsync(updateRegistrationTaskStatusRequest);

        return RedirectToAction("Index", "ManageRegistrations", new { id = registrationId });
    }

    [HttpPost]
    [Route(PagePath.CompleteQueryMaterialTask)]
    public async Task<IActionResult> CompleteQueryMaterialTask(
        int registrationMaterialId,
        string taskName,
        [Required(AllowEmptyStrings = false, ErrorMessage = "Comments must be provided")]
        [MaxLength(500, ErrorMessage = "Comments must be 500 characters or less")]
        string comments)
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.CompleteQueryMaterialTask);
        SetBackLinkInfos(session, PagePath.CompleteQueryMaterialTask);

        var updateRegistrationTaskStatusRequest = new UpdateMaterialTaskStatusRequest
        {
            TaskName = taskName,
            RegistrationMaterialId = registrationMaterialId,
            Status = RegulatorTaskStatus.Queried.ToString(),
            Comments = comments
        };

        await reprocessorExporterService.UpdateRegulatorApplicationTaskStatusAsync(updateRegistrationTaskStatusRequest);

        var registrationMaterial = await reprocessorExporterService.GetRegistrationMaterialByIdAsync(registrationMaterialId);

        return RedirectToAction("Index", "ManageRegistrations", new { id = registrationMaterial.RegistrationId });
    }

    private static string GetRegistrationMethodPath(string pagePath, int registrationId) =>
        $"{pagePath}?registrationId={registrationId}";

    private static string GetRegistrationMaterialMethodPath(string pagePath, int registrationMaterialId) =>
        $"{pagePath}?registrationMaterialId={registrationMaterialId}";
}