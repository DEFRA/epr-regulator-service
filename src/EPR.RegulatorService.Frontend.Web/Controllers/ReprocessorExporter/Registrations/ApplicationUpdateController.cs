using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Sessions;

using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

using FluentValidation;

using Microsoft.FeatureManagement.Mvc;
using Microsoft.AspNetCore.Mvc;
using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Exceptions;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations;

[FeatureGate(FeatureFlags.ReprocessorExporter)]
[Route($"{PagePath.ReprocessorExporterRegistrations}")]
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

        string pagePath = $"{PagePath.ApplicationUpdate}?registrationMaterialId={applicationUpdateSession.RegistrationMaterialId}";
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
            string pagePath = $"{PagePath.ApplicationUpdate}?registrationMaterialId={applicationUpdateSession.RegistrationMaterialId}";
            SetBackLinkInfos(session, pagePath);

            mapper.Map(applicationUpdateSession, viewModel);
            return View(GetRegistrationsView("ApplicationUpdate"), viewModel);
        }

        applicationUpdateSession.Status = viewModel.Status;

        await SaveSession(session);

        return viewModel.Status == ApplicationStatus.Granted
            ? RedirectToAction(PagePath.ApplicationGrantedDetails, "Registrations")
            : RedirectToAction(PagePath.ApplicationRefusedDetails, "Registrations");
    }

    [HttpGet]
    [Route(PagePath.ApplicationGrantedDetails)]
    public async Task<IActionResult> ApplicationGrantedDetails()
    {
        var session = await GetSession();

        var applicationUpdateSession = GetApplicationUpdateSession(session);
        var viewModel = mapper.Map<ApplicationUpdateViewModel>(applicationUpdateSession);

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
        var viewModel = mapper.Map<ApplicationUpdateViewModel>(applicationUpdateSession);

        await SaveSessionAndJourney(session, PagePath.ApplicationRefusedDetails);
        SetBackLinkInfos(session, PagePath.ApplicationRefusedDetails);

        return View(GetRegistrationsView(nameof(ApplicationRefusedDetails)), viewModel);
    }

    [HttpPost]
    [Route(PagePath.ApplicationSaveStatus)]
    public async Task<IActionResult> ApplicationSaveStatus(string? comments)
    {
        // TODO: Check that text area character count decreases as text is typed
        var session = await GetSession();

        var applicationUpdateSession = GetApplicationUpdateSession(session);

        await registrationService.SaveRegistrationMaterialStatus(applicationUpdateSession.RegistrationMaterialId, applicationUpdateSession.Status, comments);

        return RedirectToAction(PagePath.ManageRegistrations, "Registrations", new { id = applicationUpdateSession.RegistrationId });
    }

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
        ApplicationUpdateSession applicationUpdateSession;

        if (session.ReprocessorExporterSession.ApplicationUpdateSession == null)
        {
            var registrationMaterial = await registrationService.GetRegistrationMaterial(registrationMaterialId);
            applicationUpdateSession = mapper.Map<ApplicationUpdateSession>(registrationMaterial);
            session.ReprocessorExporterSession.ApplicationUpdateSession = applicationUpdateSession;
        }
        else
        {
            applicationUpdateSession = session.ReprocessorExporterSession.ApplicationUpdateSession;
        }

        return applicationUpdateSession;
    }
}