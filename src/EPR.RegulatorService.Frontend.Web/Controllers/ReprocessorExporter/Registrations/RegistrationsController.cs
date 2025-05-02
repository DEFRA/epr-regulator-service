using AutoMapper;

using EPR.Common.Authorization.Constants;
using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
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

        await SaveCurrentPageToSession(session);
        SetBackLinkInfos(session);

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

        await SaveCurrentPageToSession(session);
        SetBackLinkInfos(session);

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

        await SaveCurrentPageToSession(session);
        SetBackLinkInfos(session);

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

        await SaveCurrentPageToSession(session);
        SetBackLinkInfos(session);

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

        await SaveCurrentPageToSession(session);
        SetBackLinkInfos(session);

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

        await SaveCurrentPageToSession(session);
        SetBackLinkInfos(session);

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

        await SaveCurrentPageToSession(session);
        SetBackLinkInfos(session);

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

        await SaveCurrentPageToSession(session);
        SetBackLinkInfos(session);

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

        await SaveCurrentPageToSession(session);
        SetBackLinkInfos(session);

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

        await SaveCurrentPageToSession(session);
        SetBackLinkInfos(session);

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

        await SaveCurrentPageToSession(session);
        SetBackLinkInfos(session);

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

        await SaveCurrentPageToSession(session);
        SetBackLinkInfos(session);

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

        await SaveCurrentPageToSession(session);
        SetBackLinkInfos(session);

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
}