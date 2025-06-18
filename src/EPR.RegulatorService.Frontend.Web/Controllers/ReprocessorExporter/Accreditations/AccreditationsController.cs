namespace EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Accreditations;

using AutoMapper;

using Core.Enums;

using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using EPR.Common.Authorization.Constants;
using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Accreditations;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Models.FileDownload;
using ViewModels.ReprocessorExporter.Accreditations;

[FeatureGate(FeatureFlags.ReprocessorExporter)]
[Authorize(Policy = PolicyConstants.RegulatorBasicPolicy)]
[Route(PagePath.ReprocessorExporterAccreditations)]
public class AccreditationsController(ISessionManager<JourneySession> sessionManager,
                                    IReprocessorExporterService reprocessorExporterService,
                                    IConfiguration configuration,
                                    IMapper mapper,
                                    IValidator<IdRequest> idRequestValidator
                                    ) : ReprocessorExporterBaseController(sessionManager, configuration)
{
    [HttpGet]
    [Route(PagePath.SamplingInspection)]
    public async Task<IActionResult> SamplingInspection(Guid accreditationId, int year)
    {
        await idRequestValidator.ValidateAndThrowAsync(new IdRequest { Id = accreditationId });

        var session = await GetSession();
        InitialiseAccreditationStatusSessionIfNotExists(session, accreditationId, year);

        string pagePath = GetSamplingInspectionPagePath(PagePath.SamplingInspection, accreditationId, year);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        var samplingPlan = await reprocessorExporterService.GetSamplingPlanByAccreditationIdAsync(accreditationId);

        var model = new AccreditationSamplingInspectionViewModel()
        {
            AccreditationId = accreditationId,
            Year = year,
            AccreditationSamplingPlan = samplingPlan
        };

        return View(GetAccreditationsView(nameof(SamplingInspection)), model);
    }

    [HttpPost]
    [Route(PagePath.SamplingInspection)]
    public async Task<IActionResult> CompleteSamplingInspection(Guid accreditationId)
    {
        var session = await GetSession();

        var updateAccreditationTaskStatusRequest = new UpdateAccreditationTaskStatusRequest
        {
            TaskName = nameof(RegulatorTaskType.SamplingAndInspectionPlan),
            AccreditationId = accreditationId,
            Status = nameof(RegulatorTaskStatus.Completed)
        };

        await reprocessorExporterService.UpdateRegulatorAccreditationTaskStatusAsync(updateAccreditationTaskStatusRequest);

        return RedirectToAction("Index", "ManageAccreditations", new
        {
            id = session.ReprocessorExporterSession.RegistrationId,
            year = session.ReprocessorExporterSession.AccreditationStatusSession!.Year
        });
    }

    [HttpGet]
    public async Task<IActionResult> DownloadSamplingAndInspectionFile(Guid? fileId, string filename)
    {
        var fileDownloadModel = new FileDownloadRequest { FileId = fileId, FileName = filename, SubmissionType = SubmissionType.Accreditation };

        var response = await reprocessorExporterService.DownloadSamplingInspectionFile(fileDownloadModel);

        if (!response.IsSuccessStatusCode)
        {
            return NotFound();
        }

        var content = await response.Content.ReadAsByteArrayAsync();
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

        return File(content, contentType, filename);
    }    

    private static string GetSamplingInspectionPagePath(string pagePath, Guid accreditationId, int year) => $"{pagePath}?accreditationId={accreditationId}&year={year}";
    protected static string GetAccreditationsView(string viewName) => $"~/Views/ReprocessorExporter/Accreditations/{viewName}.cshtml";
}