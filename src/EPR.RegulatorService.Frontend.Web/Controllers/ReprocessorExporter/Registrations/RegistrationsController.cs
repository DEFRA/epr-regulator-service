using AutoMapper;

using EPR.Common.Authorization.Constants;
using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Models.FileDownload;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

using FluentValidation;
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
    IConfiguration configuration,
    IMapper mapper,
    IValidator<IdRequest> idRequestValidator)
    : ReprocessorExporterBaseController(sessionManager, configuration)
{
    [HttpGet]
    [Route(PagePath.AuthorisedMaterials)]
    public async Task<IActionResult> AuthorisedMaterials(Guid registrationId)
    {
        await idRequestValidator.ValidateAndThrowAsync(new IdRequest { Id = registrationId });

        var session = await GetSession();

        string pagePath = GetRegistrationMethodPath(PagePath.AuthorisedMaterials, registrationId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        var registrationAuthorisedMaterials =
            await reprocessorExporterService.GetAuthorisedMaterialsByRegistrationIdAsync(registrationId);
        var viewModel = mapper.Map<AuthorisedMaterialsViewModel>(registrationAuthorisedMaterials);

        await CreateQueryRegistrationSession(registrationAuthorisedMaterials, session, PagePath.AuthorisedMaterials, RegulatorTaskType.MaterialsAuthorisedOnSite);

        return View(GetRegistrationsView(nameof(AuthorisedMaterials)), viewModel);
    }

    [HttpPost]
    [Route(PagePath.AuthorisedMaterials)]
    public async Task<IActionResult> CompleteAuthorisedMaterials(Guid registrationId)
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
    public async Task<IActionResult> UkSiteDetails(Guid registrationId)
    {
        await idRequestValidator.ValidateAndThrowAsync(new IdRequest { Id = registrationId });

        var session = await GetSession();
        string pagePath = GetRegistrationMethodPath(PagePath.UkSiteDetails, registrationId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        var siteDetails = await reprocessorExporterService.GetSiteDetailsByRegistrationIdAsync(registrationId);

        await CreateQueryRegistrationSession(siteDetails, session, PagePath.UkSiteDetails, RegulatorTaskType.SiteAddressAndContactDetails);

        var viewModel = mapper.Map<SiteDetailsViewModel>(siteDetails);

        return View(GetRegistrationsView(nameof(UkSiteDetails)), viewModel);
    }

    [HttpPost]
    [Route(PagePath.UkSiteDetails)]
    public async Task<IActionResult> CompleteUkSiteDetails(Guid registrationId)
    {
        var updateRegistrationTaskStatusRequest = new UpdateRegistrationTaskStatusRequest
        {
            TaskName = RegulatorTaskType.SiteAddressAndContactDetails.ToString(),
            RegistrationId = registrationId,
            Status = RegulatorTaskStatus.Completed.ToString(),
        };

        await reprocessorExporterService.UpdateRegulatorRegistrationTaskStatusAsync(
            updateRegistrationTaskStatusRequest);

        return RedirectToAction("Index", "ManageRegistrations", new { id = registrationId });
    }
    
    [HttpGet]
    [Route(PagePath.WasteCarrierDetails)]
    public async Task<IActionResult> WasteCarrierDetails(Guid registrationId)
    {
        await idRequestValidator.ValidateAndThrowAsync(new IdRequest { Id = registrationId });

        var session = await GetSession();
        string pagePath = GetRegistrationMethodPath(PagePath.WasteCarrierDetails, registrationId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        var wasteCarrierDetails = await reprocessorExporterService.GetWasteCarrierDetailsByRegistrationIdAsync(registrationId);

        await CreateQueryRegistrationSession(wasteCarrierDetails, session, PagePath.WasteCarrierDetails, RegulatorTaskType.WasteCarrierBrokerDealerNumber);

        var viewModel = mapper.Map<WasteCarrierDetailsViewModel>(wasteCarrierDetails);

        return View(GetRegistrationsView(nameof(WasteCarrierDetails)), viewModel);
    }

    [HttpPost]
    [Route(PagePath.WasteCarrierDetails)]
    public async Task<IActionResult> CompleteWasteCarrierDetails(Guid registrationId)
    {
        var updateRegistrationTaskStatusRequest = new UpdateRegistrationTaskStatusRequest
        {
            TaskName = RegulatorTaskType.WasteCarrierBrokerDealerNumber.ToString(),
            RegistrationId = registrationId,
            Status = RegulatorTaskStatus.Completed.ToString(),
        };

        await reprocessorExporterService.UpdateRegulatorRegistrationTaskStatusAsync(
            updateRegistrationTaskStatusRequest);

        return RedirectToAction("Index", "ManageRegistrations", new { id = registrationId });
    }
    
    [HttpGet]
    [Route(PagePath.MaterialWasteLicences)]
    public async Task<IActionResult> MaterialWasteLicences(Guid registrationMaterialId)
    {
        await idRequestValidator.ValidateAndThrowAsync(new IdRequest { Id = registrationMaterialId });

        var session = await GetSession();

        string pagePath = GetRegistrationMaterialMethodPath(PagePath.MaterialWasteLicences, registrationMaterialId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        var materialWasteLicences =
            await reprocessorExporterService.GetWasteLicenceByRegistrationMaterialIdAsync(registrationMaterialId);
        var viewModel = mapper.Map<MaterialWasteLicencesViewModel>(materialWasteLicences);

        await CreateQueryMaterialSession(materialWasteLicences, session, PagePath.MaterialWasteLicences, RegulatorTaskType.WasteLicensesPermitsAndExemptions);

        return View(GetRegistrationsView(nameof(MaterialWasteLicences)), viewModel);
    }

    [HttpPost]
    [Route(PagePath.MaterialWasteLicences)]
    public async Task<IActionResult> CompleteMaterialWasteLicences(Guid registrationMaterialId)
    {
        var updateRegistrationTaskStatusRequest = new UpdateMaterialTaskStatusRequest
        {
            TaskName = RegulatorTaskType.WasteLicensesPermitsAndExemptions.ToString(),
            RegistrationMaterialId = registrationMaterialId,
            Status = RegulatorTaskStatus.Completed.ToString(),
        };

        await reprocessorExporterService.UpdateRegulatorApplicationTaskStatusAsync(updateRegistrationTaskStatusRequest);

        var registrationMaterial =
            await reprocessorExporterService.GetRegistrationMaterialByIdAsync(registrationMaterialId);

        return RedirectToAction("Index", "ManageRegistrations", new { id = registrationMaterial.RegistrationId });
    }

    [HttpGet]
    [Route(PagePath.SamplingInspection)]
    public async Task<IActionResult> SamplingInspection(Guid registrationMaterialId)
    {
        await idRequestValidator.ValidateAndThrowAsync(new IdRequest { Id = registrationMaterialId });

        var session = await GetSession();

        string pagePath = GetRegistrationMaterialMethodPath(PagePath.SamplingInspection, registrationMaterialId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        var samplingPlan =
            await reprocessorExporterService.GetSamplingPlanByRegistrationMaterialIdAsync(registrationMaterialId);
        var model = new RegistrationMaterialSamplingInspectionViewModel()
        {
            RegistrationMaterialId = registrationMaterialId, RegistrationMaterialSamplingPlan = samplingPlan
        };

        await CreateQueryMaterialSession(samplingPlan, session, PagePath.SamplingInspection, RegulatorTaskType.SamplingAndInspectionPlan);
        
        return View(GetRegistrationsView(nameof(SamplingInspection)), model);
    }

    [HttpPost]
    [Route(PagePath.SamplingInspection)]
    public async Task<IActionResult> CompleteSamplingInspection(Guid registrationMaterialId)
    {
        var updateRegistrationTaskStatusRequest = new UpdateMaterialTaskStatusRequest
        {
            TaskName = RegulatorTaskType.SamplingAndInspectionPlan.ToString(),
            RegistrationMaterialId = registrationMaterialId,
            Status = RegulatorTaskStatus.Completed.ToString(),
        };

        await reprocessorExporterService.UpdateRegulatorApplicationTaskStatusAsync(updateRegistrationTaskStatusRequest);

        var registrationMaterial =
            await reprocessorExporterService.GetRegistrationMaterialByIdAsync(registrationMaterialId);

        return RedirectToAction("Index", "ManageRegistrations", new { id = registrationMaterial.RegistrationId });
    }

    [HttpGet]
    [Route(PagePath.InputsAndOutputs)]
    public async Task<IActionResult> InputsAndOutputs(Guid registrationMaterialId)
    {
        await idRequestValidator.ValidateAndThrowAsync(new IdRequest { Id = registrationMaterialId });

        var session = await GetSession();

        string pagePath = GetRegistrationMaterialMethodPath(PagePath.InputsAndOutputs, registrationMaterialId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        var reprocessingInputsAndOutputs =
            await reprocessorExporterService.GetReprocessingIOByRegistrationMaterialIdAsync(registrationMaterialId);
        var model = new RegistrationMaterialReprocessingIOViewModel
        {
            RegistrationMaterialId = registrationMaterialId, RegistrationMaterialReprocessingIO = reprocessingInputsAndOutputs
        };

        await CreateQueryMaterialSession(reprocessingInputsAndOutputs, session, PagePath.InputsAndOutputs, RegulatorTaskType.ReprocessingInputsAndOutputs);

        return View(GetRegistrationsView(nameof(InputsAndOutputs)), model);
    }

    [HttpPost]
    [Route(PagePath.InputsAndOutputs)]
    public async Task<IActionResult> CompleteInputsAndOutputs(Guid registrationMaterialId)
    {
        var updateRegistrationTaskStatusRequest = new UpdateMaterialTaskStatusRequest
        {
            TaskName = RegulatorTaskType.ReprocessingInputsAndOutputs.ToString(),
            RegistrationMaterialId = registrationMaterialId,
            Status = RegulatorTaskStatus.Completed.ToString(),
        };

        await reprocessorExporterService.UpdateRegulatorApplicationTaskStatusAsync(updateRegistrationTaskStatusRequest);

        var registrationMaterial =
            await reprocessorExporterService.GetRegistrationMaterialByIdAsync(registrationMaterialId);

        return RedirectToAction("Index", "ManageRegistrations", new { id = registrationMaterial.RegistrationId });
    }

    [HttpGet]
    [Route(PagePath.WasteLicences)]
    public async Task<IActionResult> WasteLicences(Guid registrationId)
    {
        await idRequestValidator.ValidateAndThrowAsync(new IdRequest { Id = registrationId });

        var session = await GetSession();

        string pagePath = GetRegistrationMethodPath(PagePath.WasteLicences, registrationId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        await CreateQueryRegistrationSession(new RegistrationWasteLicences { RegistrationId = registrationId}, session, PagePath.WasteLicences, RegulatorTaskType.WasteLicensesPermitsAndExemptions);

        return View(GetRegistrationsView(nameof(WasteLicences)), registrationId);
    }

    [HttpPost]
    [Route(PagePath.WasteLicences)]
    public async Task<IActionResult> CompleteWasteLicences(Guid registrationId)
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
    public async Task<IActionResult> BusinessAddress(Guid registrationId)
    {
        await idRequestValidator.ValidateAndThrowAsync(new IdRequest { Id = registrationId });

        var session = await GetSession();

        string pagePath = GetRegistrationMethodPath(PagePath.BusinessAddress, registrationId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        await CreateQueryRegistrationSession(
            new RegistrationBusinessAddress { RegistrationId = registrationId },
            session,
            PagePath.BusinessAddress,
            RegulatorTaskType.BusinessAddress);

        return View(GetRegistrationsView(nameof(BusinessAddress)), registrationId);
    }

    [HttpPost]
    [Route(PagePath.BusinessAddress)]
    public async Task<IActionResult> CompleteBusinessAddress(Guid registrationId)
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
    public async Task<IActionResult> MaterialDetails(Guid registrationMaterialId)
    {
        await idRequestValidator.ValidateAndThrowAsync(new IdRequest { Id = registrationMaterialId });

        var session = await GetSession();

        string pagePath = GetRegistrationMaterialMethodPath(PagePath.MaterialDetails, registrationMaterialId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        var registrationMaterial = await reprocessorExporterService.GetRegistrationMaterialByIdAsync(registrationMaterialId);

        await CreateQueryMaterialSession(
            new RegistrationMaterialDetail
            {
                Id = registrationMaterial.Id,
                MaterialName = registrationMaterial.MaterialName,
                RegistrationId = registrationMaterial.RegistrationId
            }, session, PagePath.MaterialDetails, RegulatorTaskType.MaterialDetailsAndContact);

        return View(GetRegistrationsView(nameof(MaterialDetails)), registrationMaterialId);
    }

    [HttpPost]
    [Route(PagePath.MaterialDetails)]
    public async Task<IActionResult> CompleteMaterialDetails(Guid registrationMaterialId)
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
    public async Task<IActionResult> OverseasReprocessorInterim(Guid registrationMaterialId)
    {
        await idRequestValidator.ValidateAndThrowAsync(new IdRequest { Id = registrationMaterialId });

        var session = await GetSession();

        string pagePath = GetRegistrationMaterialMethodPath(PagePath.OverseasReprocessorInterim, registrationMaterialId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        var registrationMaterial = await reprocessorExporterService.GetRegistrationMaterialByIdAsync(registrationMaterialId);

        await CreateQueryMaterialSession(
            new OverseasReprocessorInterimSites
            {
                RegistrationId = registrationMaterial.RegistrationId,
                RegistrationMaterialId = registrationMaterial.Id
            },
            session,
            PagePath.OverseasReprocessorInterim,
            RegulatorTaskType.OverseasReprocessorAndInterimSiteDetails);

        return View(GetRegistrationsView(nameof(OverseasReprocessorInterim)), registrationMaterialId);
    }

    [HttpPost]
    [Route(PagePath.OverseasReprocessorInterim)]
    public async Task<IActionResult> CompleteOverseasReprocessorInterim(Guid registrationMaterialId)
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
    public async Task<IActionResult> DownloadSamplingAndInspectionFile(int registrationMaterialId, string filename,
        Guid? fileId)
    {
        var fileDownloadModel = new FileDownloadRequest { FileId = fileId, FileName = filename };

        var response = await reprocessorExporterService.DownloadSamplingInspectionFile(fileDownloadModel);

        if (!response.IsSuccessStatusCode)
        {
            return NotFound();
        }

        var content = await response.Content.ReadAsByteArrayAsync();
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

        return File(content, contentType, filename);
    }

    private async Task CreateQueryRegistrationSession<T>(T entity, JourneySession session, string returnPagePath, RegulatorTaskType taskName)
    {
        var queryRegistrationSession = mapper.Map<QueryRegistrationSession>(entity);
        queryRegistrationSession.PagePath = returnPagePath;
        queryRegistrationSession.TaskName = taskName;
        session.ReprocessorExporterSession.QueryRegistrationSession = queryRegistrationSession;
        await SaveSession(session);
    }

    private async Task CreateQueryMaterialSession<T>(T entity, JourneySession session, string returnPagePath, RegulatorTaskType taskName)
    {
        var queryMaterialSession = mapper.Map<QueryMaterialSession>(entity);
        queryMaterialSession.PagePath = returnPagePath;
        queryMaterialSession.TaskName = taskName;
        session.ReprocessorExporterSession.QueryMaterialSession = queryMaterialSession;
        await SaveSession(session);
    }

    private static string GetRegistrationMethodPath(string pagePath, Guid registrationId) =>
        $"{pagePath}?registrationId={registrationId}";

    private static string GetRegistrationMaterialMethodPath(string pagePath, Guid registrationMaterialId) =>
        $"{pagePath}?registrationMaterialId={registrationMaterialId}";
}