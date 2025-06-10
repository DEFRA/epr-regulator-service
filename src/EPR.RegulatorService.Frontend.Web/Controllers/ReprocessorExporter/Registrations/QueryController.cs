using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Exceptions;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.Query;

using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations;

[FeatureGate(FeatureFlags.ReprocessorExporter)]
[Route(PagePath.ReprocessorExporterRegistrations)]
public class QueryController (
    IMapper mapper,
    IReprocessorExporterService reprocessorExporterService,
    ISessionManager<JourneySession> sessionManager,
    IConfiguration configuration)
    : ReprocessorExporterBaseController(sessionManager, configuration)
{
    private const string RegistrationNoteFormAction = "AddRegistrationQueryNote";
    private const string MaterialNoteFormAction = "AddMaterialQueryNote";
    private const string AddQueryNoteView = "~/Views/ReprocessorExporter/Registrations/Query/AddQueryNote.cshtml";

    [HttpGet]
    [Route(PagePath.AddRegistrationQueryNote)]

    public async Task<IActionResult> AddRegistrationQueryNote()
    {
        var session = await GetSession();
        var queryRegistrationSession = GetQueryRegistrationSession(session);

        await SaveSessionAndJourney(session, PagePath.AddRegistrationQueryNote);
        SetBackLinkInfos(session, PagePath.AddRegistrationQueryNote);

        var viewModel = mapper.Map<AddQueryNoteViewModel>(queryRegistrationSession);
        viewModel.FormAction = RegistrationNoteFormAction;

        return View(AddQueryNoteView, viewModel);
    }

    [HttpPost]
    [Route(PagePath.AddRegistrationQueryNote)]

    public async Task<IActionResult> AddRegistrationQueryNote(AddQueryNoteViewModel viewModel)
    {
        var session = await GetSession();
        var queryRegistrationSession = GetQueryRegistrationSession(session);

        if (!ModelState.IsValid)
        {
            SetBackLinkInfos(session, PagePath.AddMaterialQueryNote);
            mapper.Map(queryRegistrationSession, viewModel);
            viewModel.FormAction = RegistrationNoteFormAction;
            return View(AddQueryNoteView, viewModel);
        }

        await reprocessorExporterService.AddRegistrationQueryNoteAsync(queryRegistrationSession.RegulatorRegistrationTaskStatusId, new AddNoteRequest { Note = viewModel.Note! });

        return RedirectToAction(queryRegistrationSession.PagePath, PagePath.ReprocessorExporterRegistrations, new { registrationId = queryRegistrationSession.RegistrationId });
    }

    [HttpGet]
    [Route(PagePath.AddMaterialQueryNote)]

    public async Task<IActionResult> AddMaterialQueryNote()
    {
        var session = await GetSession();
        var queryMaterialSession = GetQueryMaterialSession(session);

        await SaveSessionAndJourney(session, PagePath.AddMaterialQueryNote);
        SetBackLinkInfos(session, PagePath.AddMaterialQueryNote);
        
        var viewModel = mapper.Map<AddQueryNoteViewModel>(queryMaterialSession);
        viewModel.FormAction = MaterialNoteFormAction;
        
        return View(AddQueryNoteView, viewModel);
    }

    [HttpPost]
    [Route(PagePath.AddMaterialQueryNote)]

    public async Task<IActionResult> AddMaterialQueryNote(AddQueryNoteViewModel viewModel)
    {
        var session = await GetSession();
        var queryMaterialSession = GetQueryMaterialSession(session);

        if (!ModelState.IsValid)
        {
            SetBackLinkInfos(session, PagePath.AddMaterialQueryNote);
            mapper.Map(queryMaterialSession, viewModel);
            viewModel.FormAction = MaterialNoteFormAction;

            return View(AddQueryNoteView, viewModel);
        }

        await reprocessorExporterService.AddMaterialQueryNoteAsync(queryMaterialSession.RegulatorApplicationTaskStatusId, new AddNoteRequest {Note = viewModel.Note! });

        return RedirectToAction(queryMaterialSession.PagePath, PagePath.ReprocessorExporterRegistrations, new { registrationMaterialId = queryMaterialSession.RegistrationMaterialId});
    }

    private static QueryRegistrationSession GetQueryRegistrationSession(JourneySession session)
    {
        if (session.ReprocessorExporterSession.QueryRegistrationSession == null)
        {
            throw new SessionException("QueryRegistrationSession does not exist");
        }

        return session.ReprocessorExporterSession.QueryRegistrationSession;
    }

    private static QueryMaterialSession GetQueryMaterialSession(JourneySession session)
    {
        if (session.ReprocessorExporterSession.QueryMaterialSession == null)
        {
            throw new SessionException("QueryMaterialSession does not exist");
        }

        return session.ReprocessorExporterSession.QueryMaterialSession;
    }
}