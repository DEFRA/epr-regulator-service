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
    [HttpGet]
    [Route(PagePath.AddMaterialQueryNote)]

    public async Task<IActionResult> AddMaterialQueryNote()
    {
        var session = await GetSession();
        var queryMaterialSession = GetQueryMaterialSessionSession(session);

        await SaveSessionAndJourney(session, PagePath.AddMaterialQueryNote);
        SetBackLinkInfos(session, PagePath.AddMaterialQueryNote);
        
        var viewModel = mapper.Map<AddMaterialQueryNoteViewModel>(queryMaterialSession);
        
        return View(GetQueryView(nameof(AddMaterialQueryNote)), viewModel);
    }

    [HttpPost]
    [Route(PagePath.AddMaterialQueryNote)]

    public async Task<IActionResult> AddMaterialQueryNote(AddMaterialQueryNoteViewModel viewModel)
    {
        var session = await GetSession();
        var queryMaterialSession = GetQueryMaterialSessionSession(session);

        if (!ModelState.IsValid)
        {
            SetBackLinkInfos(session, PagePath.AddMaterialQueryNote);
            mapper.Map(queryMaterialSession, viewModel);
            return View(GetQueryView(nameof(AddMaterialQueryNote)), viewModel);
        }

        await reprocessorExporterService.AddMaterialQueryNoteAsync(queryMaterialSession.RegulatorApplicationTaskStatusId, new AddNoteRequest {Note = viewModel.Note! });

        return RedirectToAction(queryMaterialSession.PagePath, PagePath.ReprocessorExporterRegistrations, new { registrationMaterialId = queryMaterialSession.RegistrationMaterialId});
    }

    private static QueryMaterialSession GetQueryMaterialSessionSession(JourneySession session)
    {
        if (session.ReprocessorExporterSession.QueryMaterialSession == null)
        {
            throw new SessionException("QueryMaterialSession does not exist");
        }

        return session.ReprocessorExporterSession.QueryMaterialSession;
    }

    protected static string GetQueryView(string viewName) => $"~/Views/ReprocessorExporter/Registrations/Query/{viewName}.cshtml";
}