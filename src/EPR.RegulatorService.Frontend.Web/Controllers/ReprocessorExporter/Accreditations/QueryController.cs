using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Exceptions;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Accreditations;
using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.Query;

using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Accreditations;

[FeatureGate(FeatureFlags.ReprocessorExporter)]
[Route(PagePath.ReprocessorExporterRegistrations)]
public class QueryController (
    IMapper mapper,
    IReprocessorExporterService reprocessorExporterService,
    ISessionManager<JourneySession> sessionManager,
    IConfiguration configuration)
    : ReprocessorExporterBaseController(sessionManager, configuration)
{
    private const string AddQueryNoteView = "~/Views/ReprocessorExporter/AddQueryNote.cshtml";

    [HttpGet]
    [Route(PagePath.QueryAccreditationTask)]

    public async Task<IActionResult> QueryAccreditationTask()
    {
        var session = await GetSession();
        var queryAccreditationSession = GetQueryAccreditationSession(session);

        await SaveSessionAndJourney(session, PagePath.QueryAccreditationTask);
        SetBackLinkInfos(session, PagePath.QueryAccreditationTask);

        var viewModel = mapper.Map<AddQueryNoteViewModel>(queryAccreditationSession);
        viewModel.FormAction = nameof(QueryAccreditationTask);

        return View(AddQueryNoteView, viewModel);
    }

    [HttpPost]
    [Route(PagePath.QueryAccreditationTask)]

    public async Task<IActionResult> QueryAccreditationTask(AddQueryNoteViewModel viewModel)
    {
        var session = await GetSession();
        var queryAccreditationSession = GetQueryAccreditationSession(session);

        if (!ModelState.IsValid)
        {
            SetBackLinkInfos(session, PagePath.QueryAccreditationTask);
            mapper.Map(queryAccreditationSession, viewModel);
            viewModel.FormAction = nameof(QueryAccreditationTask);
            return View(AddQueryNoteView, viewModel);
        }

        await reprocessorExporterService.UpdateRegulatorAccreditationTaskStatusAsync(
            new UpdateAccreditationTaskStatusRequest
            {
                Comments = viewModel.Note,
                AccreditationId = queryAccreditationSession.AccreditationId,
                Status = RegulatorTaskStatus.Queried.ToString(),
                TaskName = queryAccreditationSession.TaskName.ToString()
            });

        return RedirectToAction("Index", "ManageAccreditations", new { Id = queryAccreditationSession.RegistrationId, Year = queryAccreditationSession.Year });
    }

    private static QueryAccreditationSession GetQueryAccreditationSession(JourneySession session)
    {
        if (session.ReprocessorExporterSession.QueryAccreditationSession == null)
        {
            throw new SessionException("QueryAccreditationSession does not exist");
        }

        return session.ReprocessorExporterSession.QueryAccreditationSession;
    }
}