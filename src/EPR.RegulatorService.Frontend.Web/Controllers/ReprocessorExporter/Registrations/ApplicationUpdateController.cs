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

using System.Diagnostics.CodeAnalysis;

using Core.Models.ReprocessorExporter.Registrations;

[ExcludeFromCodeCoverage]
[FeatureGate(FeatureFlags.ReprocessorExporter)]
[Route(PagePath.ReprocessorExporterRegistrations)]
public class ApplicationUpdateController(
    IMapper mapper,
    IValidator<IdRequest> validator,
    IReprocessorExporterService reprocessorExporterService,
    ISessionManager<JourneySession> sessionManager,
    IConfiguration configuration)
    : ReprocessorExporterBaseController(sessionManager, configuration)
{
    [HttpGet]
    [Route(PagePath.ApplicationUpdate)]
    public async Task<IActionResult> ApplicationUpdate(int registrationMaterialId)
    {
        await validator.ValidateAndThrowAsync(new IdRequest { Id = registrationMaterialId });

        var session = await GetSession();

        var applicationUpdateSession = await GetOrCreateApplicationUpdateSession(registrationMaterialId, session);
        var viewModel = mapper.Map<ApplicationUpdateViewModel>(applicationUpdateSession);

        string pagePath = GetApplicationUpdatePath(applicationUpdateSession.RegistrationMaterialId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        return View(GetRegistrationsView(nameof(ApplicationUpdate)), viewModel);
    }

    [HttpPost]
    [Route(PagePath.ApplicationUpdate)]
    public async Task<IActionResult> ApplicationUpdate(ApplicationUpdateViewModel viewModel)
    {
        var session = await GetSession();
        var applicationUpdateSession = GetApplicationUpdateSession(session);

        if (!ModelState.IsValid)
        {
            return HandleInvalidModelState(
                session,
                GetApplicationUpdatePath(applicationUpdateSession.RegistrationMaterialId),
                applicationUpdateSession,
                viewModel,
                GetRegistrationsView(nameof(ApplicationUpdate))
                );
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

    [HttpPost]
    [Route(PagePath.ApplicationGrantedDetails)]
    public async Task<IActionResult> ApplicationGrantedDetails(ApplicationGrantedViewModel viewModel)
    {
        var session = await GetSession();
        var applicationUpdateSession = GetApplicationUpdateSession(session);

        if (!ModelState.IsValid)
        {
            return HandleInvalidModelState(
                session,
                PagePath.ApplicationGrantedDetails,
                applicationUpdateSession,
                viewModel,
                GetRegistrationsView(nameof(ApplicationGrantedDetails))
            );
        }

        await reprocessorExporterService.UpdateRegistrationMaterialOutcomeAsync(
            applicationUpdateSession.RegistrationMaterialId,
            new RegistrationMaterialOutcomeRequest { Status = applicationUpdateSession.Status, Comments = viewModel.Comments });

        return RedirectToAction(PagePath.ManageRegistrations, PagePath.ReprocessorExporterRegistrations, new { id = applicationUpdateSession.RegistrationId });
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
    [Route(PagePath.ApplicationRefusedDetails)]
    public async Task<IActionResult> ApplicationRefusedDetails(ApplicationRefusedViewModel viewModel)
    {
        var session = await GetSession();
        var applicationUpdateSession = GetApplicationUpdateSession(session);

        if (!ModelState.IsValid)
        {
            return HandleInvalidModelState(
                session,
                PagePath.ApplicationRefusedDetails,
                applicationUpdateSession,
                viewModel,
                GetRegistrationsView(nameof(ApplicationRefusedDetails))
            );
        }

        await reprocessorExporterService.UpdateRegistrationMaterialOutcomeAsync(
            applicationUpdateSession.RegistrationMaterialId,
            new RegistrationMaterialOutcomeRequest { Status = applicationUpdateSession.Status, Comments = viewModel.Comments });

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
            var registrationMaterial = await reprocessorExporterService.GetRegistrationMaterialByIdAsync(registrationMaterialId);
            var applicationUpdateSession = mapper.Map<ApplicationUpdateSession>(registrationMaterial);

            session.ReprocessorExporterSession.ApplicationUpdateSession = applicationUpdateSession;
        }

        return session.ReprocessorExporterSession.ApplicationUpdateSession;
    }

    private IActionResult HandleInvalidModelState<T>(JourneySession session, string pagePath, ApplicationUpdateSession applicationUpdateSession, T viewModel, string viewName)
    {
        SetBackLinkInfos(session, pagePath);

        mapper.Map(applicationUpdateSession, viewModel);

        return View(viewName, viewModel);
    }
}