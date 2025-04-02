using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Exceptions;
using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

using FluentValidation;

using Microsoft.FeatureManagement.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations;

[FeatureGate(FeatureFlags.ReprocessorExporter)]
[Route(PagePath.ReprocessorExporterRegistrations)]
public class ApplicationUpdateController(
    IMapper mapper,
    IValidator<IdRequest> validator,
    IRegistrationService registrationService,
    ISessionManager<JourneySession> sessionManager,
    IConfiguration configuration)
    : ReprocessorExporterBaseController(sessionManager, configuration)
{
    [HttpGet]
    [Route(PagePath.ApplicationUpdate)]
    public async Task<IActionResult> Index([FromQuery] int registrationMaterialId)
    {
        await validator.ValidateAndThrowAsync(new IdRequest { Id = registrationMaterialId });

        var session = await GetSession();

        var applicationUpdateSession = await GetOrCreateApplicationUpdateSession(registrationMaterialId, session);
        var viewModel = mapper.Map<ApplicationUpdateViewModel>(applicationUpdateSession);

        string pagePath = GetApplicationUpdatePath(applicationUpdateSession.RegistrationMaterialId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        return View(GetRegistrationsView("ApplicationUpdate"), viewModel);
    }

    [HttpPost]
    [Route(PagePath.ApplicationUpdate)]
    public async Task<IActionResult> Index(ApplicationUpdateViewModel viewModel)
    {
        var session = await GetSession();
        var applicationUpdateSession = GetApplicationUpdateSession(session);

        if (!ModelState.IsValid)
        {
            string pagePath = GetApplicationUpdatePath(applicationUpdateSession.RegistrationMaterialId);
            SetBackLinkInfos(session, pagePath);

            mapper.Map(applicationUpdateSession, viewModel);
            return View(GetRegistrationsView("ApplicationUpdate"), viewModel);
        }

        applicationUpdateSession.Status = viewModel.Status;

        await SaveSession(session);

        return viewModel.Status == ApplicationStatus.Granted
            ? RedirectToAction(PagePath.ApplicationGrantedDetails, PagePath.ReprocessorExporterRegistrations)
            : RedirectToAction(PagePath.ApplicationRefusedDetails, PagePath.ReprocessorExporterRegistrations);
    }

    [HttpGet]
    [Route(PagePath.ApplicationGrantedDetails)]
    public async Task<IActionResult> ApplicationGrantedDetails()
    {
        var session = await GetSession();

        var applicationUpdateSession = GetApplicationUpdateSession(session);
        var viewModel = mapper.Map<ApplicationGrantedViewModel>(applicationUpdateSession);

        await SaveSessionAndJourney(session, PagePath.ApplicationGrantedDetails);
        SetBackLinkInfos(session, PagePath.ApplicationGrantedDetails);

        return View(GetRegistrationsView(nameof(ApplicationGrantedDetails)), viewModel);
    }

    [HttpGet]
    [Route(PagePath.ApplicationRefusedDetails)]
    public async Task<IActionResult> ApplicationRefusedDetails()
    {
        var session = await GetSession();

        var applicationUpdateSession = GetApplicationUpdateSession(session);
        var viewModel = mapper.Map<ApplicationRefusedViewModel>(applicationUpdateSession);

        await SaveSessionAndJourney(session, PagePath.ApplicationRefusedDetails);
        SetBackLinkInfos(session, PagePath.ApplicationRefusedDetails);

        return View(GetRegistrationsView(nameof(ApplicationRefusedDetails)), viewModel);
    }

    [HttpPost]
    [Route(PagePath.SaveGrantedApplication)]
    public async Task<IActionResult> SaveGrantedApplication(ApplicationGrantedViewModel viewModel)
    {
        var session = await GetSession();
        var applicationUpdateSession = GetApplicationUpdateSession(session);

        if (!ModelState.IsValid)
        {
            SetBackLinkInfos(session, PagePath.ApplicationGrantedDetails);

            mapper.Map(applicationUpdateSession, viewModel);
            return View(GetRegistrationsView("ApplicationGrantedDetails"), viewModel);
        }

        await registrationService.UpdateRegistrationMaterialOutcomeAsync(applicationUpdateSession.RegistrationMaterialId, applicationUpdateSession.Status, viewModel.Comments);

        return RedirectToAction(PagePath.ManageRegistrations, PagePath.ReprocessorExporterRegistrations, new { id = applicationUpdateSession.RegistrationId });
    }

    [HttpPost]
    [Route(PagePath.SaveRefusedApplication)]
    public async Task<IActionResult> SaveRefusedApplication(ApplicationRefusedViewModel viewModel)
    {
        var session = await GetSession();
        var applicationUpdateSession = GetApplicationUpdateSession(session);

        if (!ModelState.IsValid)
        {
            SetBackLinkInfos(session, PagePath.ApplicationRefusedDetails);

            mapper.Map(applicationUpdateSession, viewModel);
            return View(GetRegistrationsView("ApplicationRefusedDetails"), viewModel);
        }

        await registrationService.UpdateRegistrationMaterialOutcomeAsync(applicationUpdateSession.RegistrationMaterialId, applicationUpdateSession.Status, viewModel.Comments);

        return RedirectToAction(PagePath.ManageRegistrations, PagePath.ReprocessorExporterRegistrations, new { id = applicationUpdateSession.RegistrationId });
    }

    private static string GetApplicationUpdatePath(int registrationMaterialId) =>
        $"{PagePath.ApplicationUpdate}?registrationMaterialId={registrationMaterialId}";

    private static ApplicationUpdateSession GetApplicationUpdateSession(JourneySession session)
    {
        if (session.ReprocessorExporterSession.ApplicationUpdateSession == null)
        {
            throw new SessionException("ApplicationUpdateSession does not exist");
        }

        return session.ReprocessorExporterSession.ApplicationUpdateSession;
    }

    private async Task<ApplicationUpdateSession> GetOrCreateApplicationUpdateSession(int registrationMaterialId, JourneySession session)
    {
        if (session.ReprocessorExporterSession.ApplicationUpdateSession == null)
        {
            var registrationMaterial = await registrationService.GetRegistrationMaterialAsync(registrationMaterialId);
            var applicationUpdateSession = mapper.Map<ApplicationUpdateSession>(registrationMaterial);

            session.ReprocessorExporterSession.ApplicationUpdateSession = applicationUpdateSession;
        }

        return session.ReprocessorExporterSession.ApplicationUpdateSession;
    }
}