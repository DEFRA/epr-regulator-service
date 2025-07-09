using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using EPR.Common.Authorization.Constants;
using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Models.FileDownload;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Accreditations;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations;

namespace EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Accreditations;

[FeatureGate(FeatureFlags.ReprocessorExporter)]
[Authorize(Policy = PolicyConstants.RegulatorBasicPolicy)]
[Route(PagePath.ReprocessorExporterAccreditations)]
public class AccreditationsController(ISessionManager<JourneySession> sessionManager,
                                    IReprocessorExporterService reprocessorExporterService,
                                    IConfiguration configuration,
                                    IMapper mapper,
                                    IValidator<IdRequest> idRequestValidator
                                    ) : AccreditationBaseController(sessionManager, configuration)
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

        // The values for QueryAccreditationSession need to be returned as part of the API response (including Org Name and Site Address)
        await CreateQueryAccreditationSession(session, RegulatorTaskType.SamplingAndInspectionPlan, accreditationId, session.ReprocessorExporterSession.RegistrationId, year);

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

    private static void InitialiseAccreditationStatusSessionIfNotExists(JourneySession session, Guid accreditationId, int year)
    {
        if (session.ReprocessorExporterSession.AccreditationStatusSession != null &&
            session.ReprocessorExporterSession.AccreditationStatusSession!.AccreditationId == accreditationId &&
            session.ReprocessorExporterSession.AccreditationStatusSession!.Year == year)
        {
            return;
        }

        var accreditationStatusSession = new AccreditationStatusSession
        {
            AccreditationId = accreditationId,
            Year = year,
            OrganisationName = null!,
            MaterialName = null!
        };

        session.ReprocessorExporterSession.AccreditationStatusSession = accreditationStatusSession;
    }

    private static string GetSamplingInspectionPagePath(string pagePath, Guid accreditationId, int year) => $"{pagePath}?accreditationId={accreditationId}&year={year}";
    protected static string GetAccreditationsView(string viewName) => $"~/Views/ReprocessorExporter/Accreditations/{viewName}.cshtml";

    private async Task CreateQueryAccreditationSession(JourneySession session, RegulatorTaskType taskName, Guid accreditationId, Guid registrationId, int year)
    {
        var queryAccreditationSession = new QueryAccreditationSession
        {
            AccreditationId = accreditationId,
            TaskName = taskName,
            RegistrationId = registrationId,
            Year = year
        };
        
        session.ReprocessorExporterSession.QueryAccreditationSession = queryAccreditationSession;
        await SaveSession(session);
    }
}