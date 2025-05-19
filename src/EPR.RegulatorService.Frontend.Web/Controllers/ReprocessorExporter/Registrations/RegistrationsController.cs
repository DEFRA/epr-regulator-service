using AutoMapper;

using EPR.Common.Authorization.Constants;
using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Models.FileDownload;
using EPR.RegulatorService.Frontend.Core.Models.Registrations;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

using static StackExchange.Redis.Role;

namespace EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations;

[FeatureGate(FeatureFlags.ReprocessorExporter)]
[Authorize(Policy = PolicyConstants.RegulatorBasicPolicy)]
[Route(PagePath.ReprocessorExporterRegistrations)]
public class RegistrationsController(
    ISessionManager<JourneySession> sessionManager,
    IReprocessorExporterService reprocessorExporterService,
    IConfiguration configuration,
    IMapper mapper)
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

        var registrationAuthorisedMaterials = await reprocessorExporterService.GetAuthorisedMaterialsByRegistrationIdAsync(registrationId);
        var viewModel = mapper.Map<AuthorisedMaterialsViewModel>(registrationAuthorisedMaterials);
        
        return View(GetRegistrationsView(nameof(AuthorisedMaterials)), viewModel);
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

        var siteDetails = await reprocessorExporterService.GetSiteDetailsByRegistrationIdAsync(registrationId);
        var viewModel = mapper.Map<SiteDetailsViewModel>(siteDetails);

        return View(GetRegistrationsView(nameof(UkSiteDetails)), viewModel);
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

        var materialWasteLicences = await reprocessorExporterService.GetWasteLicenceByRegistrationMaterialIdAsync(registrationMaterialId);
        var viewModel = mapper.Map<MaterialWasteLicencesViewModel>(materialWasteLicences);

        return View(GetRegistrationsView(nameof(MaterialWasteLicences)), viewModel);
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

        var samplingPlan = await reprocessorExporterService.GetSamplingPlanByRegistrationMaterialIdAsync(registrationMaterialId);
        var model = new RegistrationMaterialSamplingInspectionViewModel()
        {
            RegistrationMaterialId = registrationMaterialId,
            RegistrationMaterialSamplingPlan = samplingPlan
        };

        return View(GetRegistrationsView(nameof(SamplingInspection)), model);
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

        var reprocessingIO = await reprocessorExporterService.GetReprocessingIOByRegistrationMaterialIdAsync(registrationMaterialId);
        var model = new RegistrationMaterialReprocessingIOViewModel()
        {
            RegistrationMaterialId = registrationMaterialId,
            RegistrationMaterialReprocessingIO = reprocessingIO
        };        

        return View(GetRegistrationsView(nameof(InputsAndOutputs)), model);     
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

    [HttpPost]
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

    [HttpPost]
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

    [HttpGet]
    [Route(PagePath.QueryRegistrationTask)]
    public async Task<IActionResult> QueryRegistrationTask(int registrationId, RegulatorTaskType taskName)
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.QueryRegistrationTask);
        SetBackLinkInfos(session, PagePath.QueryRegistrationTask);

        var queryRegistrationTaskViewModel = new QueryRegistrationTaskViewModel
        {
            RegistrationId = registrationId,
            TaskName = taskName
        };

        return View(GetRegistrationsView(nameof(QueryRegistrationTask)), queryRegistrationTaskViewModel );
    }

    [HttpPost]
    [Route(PagePath.QueryRegistrationTask)]
    public async Task<IActionResult> QueryRegistrationTask(QueryRegistrationTaskViewModel queryRegistrationTaskViewModel)
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.CompleteQueryRegistrationTask);
        SetBackLinkInfos(session, PagePath.CompleteQueryRegistrationTask);

        var updateRegistrationTaskStatusRequest = new UpdateRegistrationTaskStatusRequest
        {
            TaskName = queryRegistrationTaskViewModel.TaskName.ToString(),
            RegistrationId = queryRegistrationTaskViewModel.RegistrationId,
            Status = RegulatorTaskStatus.Queried.ToString(),
            Comments = queryRegistrationTaskViewModel.Comments
        };

        await reprocessorExporterService.UpdateRegulatorRegistrationTaskStatusAsync(updateRegistrationTaskStatusRequest);

        return RedirectToAction("Index", "ManageRegistrations", new { id = queryRegistrationTaskViewModel.RegistrationId });
    }

    [HttpGet]
    [Route(PagePath.QueryMaterialTask)]
    public async Task<IActionResult> QueryMaterialTask(int registrationMaterialId, RegulatorTaskType taskName)
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.QueryMaterialTask);
        SetBackLinkInfos(session, PagePath.QueryMaterialTask);

        var queryMaterialTaskViewModel = new QueryMaterialTaskViewModel
        {
            RegistrationMaterialId = registrationMaterialId,
            TaskName = taskName
        };

        return View(GetRegistrationsView(nameof(QueryMaterialTask)), queryMaterialTaskViewModel);
    }

    [HttpPost]
    [Route(PagePath.QueryMaterialTask)]
    public async Task<IActionResult> QueryMaterialTask(QueryMaterialTaskViewModel queryMaterialTaskViewModel)
    {
        var session = await GetSession();

        if (!ModelState.IsValid)
        {
            SetBackLinkInfos(session, PagePath.QueryMaterialTask);
            return View(GetRegistrationsView(nameof(QueryMaterialTask)), queryMaterialTaskViewModel);
        }

        var updateRegistrationTaskStatusRequest = new UpdateMaterialTaskStatusRequest
        {
            TaskName = queryMaterialTaskViewModel.TaskName.ToString(),
            RegistrationMaterialId = queryMaterialTaskViewModel.RegistrationMaterialId,
            Status = RegulatorTaskStatus.Queried.ToString(),
            Comments = queryMaterialTaskViewModel.Comments
        };

        await reprocessorExporterService.UpdateRegulatorApplicationTaskStatusAsync(updateRegistrationTaskStatusRequest);

        var registrationMaterial = await reprocessorExporterService.GetRegistrationMaterialByIdAsync(queryMaterialTaskViewModel.RegistrationMaterialId);

        return RedirectToAction("Index", "ManageRegistrations", new { id = registrationMaterial.RegistrationId });
    }

    [HttpGet]
    public async Task<IActionResult> DownloadSamplingAndInspectionFile(int registrationMaterialId, string filename, Guid? fileId)
    {
        var fileDownloadModel = new FileDownloadRequest
        {
            FileId = fileId,
            FileName = filename
        };

        var response = await reprocessorExporterService.DownloadSamplingInspectionFile(fileDownloadModel);

        if(!response.IsSuccessStatusCode)
        {
            return NotFound();
        }

        var content = await response.Content.ReadAsByteArrayAsync();
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        var contentDisposition = response.Content.Headers.ContentDisposition?.FileName ?? filename;

        return File(content, contentType, contentDisposition);
    }

    private static string GetRegistrationMethodPath(string pagePath, int registrationId) =>
        $"{pagePath}?registrationId={registrationId}";

    private static string GetRegistrationMaterialMethodPath(string pagePath, int registrationMaterialId) =>
        $"{pagePath}?registrationMaterialId={registrationMaterialId}";
  }