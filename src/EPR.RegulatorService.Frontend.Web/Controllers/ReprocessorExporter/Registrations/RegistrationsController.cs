using AutoMapper;

using EPR.Common.Authorization.Constants;
using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Exceptions;
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
    IMapper mapper,
    IValidator<IdRequest> validator,
    IRegistrationService registrationService,
    ISessionManager<JourneySession> sessionManager,
    IConfiguration configuration)
    : ReprocessorExporterBaseController(sessionManager, configuration)
{
    [HttpGet]
    [Route(PagePath.AuthorisedMaterials)]
    public async Task<IActionResult> AuthorisedMaterials()
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.AuthorisedMaterials);
        SetBackLinkInfos(session, PagePath.AuthorisedMaterials);

        var model = new ManageRegistrationsViewModel
        {
            ApplicationOrganisationType = ApplicationOrganisationType.Reprocessor
        };
        return View(RegistrationsView(nameof(AuthorisedMaterials)), model);
    }

    [HttpGet]
    [Route(PagePath.UkSiteDetails)]
    public async Task<IActionResult> UkSiteDetails()
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.UkSiteDetails);

        SetBackLinkInfos(session, PagePath.UkSiteDetails);

        var model = new ManageRegistrationsViewModel
        {
            ApplicationOrganisationType = ApplicationOrganisationType.Reprocessor
        };

        return View(RegistrationsView(nameof(UkSiteDetails)), model);
    }

    [HttpGet]
    [Route(PagePath.MaterialWasteLicences)]
    public async Task<IActionResult> MaterialWasteLicences()
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.MaterialWasteLicences);
        SetBackLinkInfos(session, PagePath.MaterialWasteLicences);

        var model = new ManageRegistrationsViewModel
        {
            ApplicationOrganisationType = ApplicationOrganisationType.Reprocessor
        };

        return View(RegistrationsView(nameof(MaterialWasteLicences)), model);
    }

    [HttpGet]
    [Route(PagePath.SamplingInspection)]
    public async Task<IActionResult> SamplingInspection()
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.SamplingInspection);
        SetBackLinkInfos(session, PagePath.SamplingInspection);

        var model = new ManageRegistrationsViewModel
        {
            ApplicationOrganisationType = ApplicationOrganisationType.Reprocessor
        };

        return View(RegistrationsView(nameof(SamplingInspection)), model);
    }
    
    [HttpGet]
    [Route(PagePath.InputsAndOutputs)]
    public async Task<IActionResult> InputsAndOutputs()
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.InputsAndOutputs);
        SetBackLinkInfos(session, PagePath.InputsAndOutputs);

        var model = new ManageRegistrationsViewModel
        {
            ApplicationOrganisationType = ApplicationOrganisationType.Reprocessor
        };

        return View(RegistrationsView(nameof(InputsAndOutputs)), model);
    }

    [HttpGet]
    [Route(PagePath.WasteLicences)]
    public async Task<IActionResult> WasteLicences()
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.WasteLicences);
        SetBackLinkInfos(session, PagePath.WasteLicences);

        var model = new ManageRegistrationsViewModel
        {
            ApplicationOrganisationType = ApplicationOrganisationType.Exporter
        };

        return View(RegistrationsView(nameof(WasteLicences)), model);
    }

    [HttpGet]
    [Route(PagePath.BusinessAddress)]
    public async Task<IActionResult> BusinessAddress()
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.BusinessAddress);
        SetBackLinkInfos(session, PagePath.BusinessAddress);

        var model = new ManageRegistrationsViewModel
        {
            ApplicationOrganisationType = ApplicationOrganisationType.Exporter
        };

        return View(RegistrationsView(nameof(BusinessAddress)), model);
    }

    [HttpGet]
    [Route(PagePath.MaterialDetails)]
    public async Task<IActionResult> MaterialDetails()
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.MaterialDetails);
        SetBackLinkInfos(session, PagePath.MaterialDetails);

        var model = new ManageRegistrationsViewModel
        {
            ApplicationOrganisationType = ApplicationOrganisationType.Exporter
        };

        return View(RegistrationsView(nameof(MaterialDetails)), model);
    }

    [HttpGet]
    [Route(PagePath.OverseasReprocessorInterim)]
    public async Task<IActionResult> OverseasReprocessorInterim()
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.OverseasReprocessorInterim);
        SetBackLinkInfos(session, PagePath.OverseasReprocessorInterim);

        var model = new ManageRegistrationsViewModel
        {
            ApplicationOrganisationType = ApplicationOrganisationType.Exporter
        };

        return View(RegistrationsView(nameof(OverseasReprocessorInterim)), model);
    }

    [HttpGet]
    [Route(PagePath.ApplicationUpdate)]
    public async Task<IActionResult> ApplicationUpdate([FromQuery]int registrationMaterialId)
    {
        await validator.ValidateAndThrowAsync(new IdRequest { Id = registrationMaterialId });

        var session = await GetSession();

        var applicationUpdateSession = await GetOrCreateApplicationUpdateSession(registrationMaterialId, session);
        var viewModel = mapper.Map<ApplicationUpdateViewModel>(applicationUpdateSession);

        string pagePath = $"{PagePath.ApplicationUpdate}?registrationMaterialId={applicationUpdateSession.RegistrationMaterialId}";
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        return View(RegistrationsView(nameof(ApplicationUpdate)), viewModel);
    }

    [HttpPost]
    [Route(PagePath.ApplicationUpdate)]
    public async Task<IActionResult> ApplicationUpdate(ApplicationUpdateViewModel viewModel)
    {
        var session = await GetSession();
        var applicationUpdateSession = GetApplicationUpdateSession(session);

        if (!ModelState.IsValid)
        {
            string pagePath = $"{PagePath.ApplicationUpdate}?registrationMaterialId={applicationUpdateSession.RegistrationMaterialId}";
            SetBackLinkInfos(session, pagePath);

            mapper.Map(applicationUpdateSession, viewModel);
            return View(RegistrationsView(nameof(ApplicationUpdate)), viewModel);
        }
        
        applicationUpdateSession.Status = viewModel.Status;

        await SaveSession(session);

        return viewModel.Status == ApplicationStatus.Granted
            ? RedirectToAction(PagePath.ApplicationGrantedDetails)
            : RedirectToAction(PagePath.ApplicationRefusedDetails);
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

        return View(RegistrationsView(nameof(ApplicationGrantedDetails)), viewModel);
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

        return View(RegistrationsView(nameof(ApplicationRefusedDetails)), viewModel);
    }

    [HttpPost]
    [Route(PagePath.ApplicationSaveStatus)]
    public async Task<IActionResult> ApplicationSaveStatus(string? comments)
    {
        // TODO: Check that text area character count decreases as text is typed
        var session = await GetSession();

        var applicationUpdateSession = GetApplicationUpdateSession(session);

        await registrationService.SaveRegistrationMaterialStatus(applicationUpdateSession.RegistrationMaterialId, applicationUpdateSession.Status, comments);

        return RedirectToAction(PagePath.ManageRegistrations, new { id = applicationUpdateSession.RegistrationId });
    }

    private void SetBackLinkInfos(JourneySession session, string currentPagePath)
    {
        if (string.IsNullOrEmpty(Request.Headers?.Referer))
            SetHomeBackLink();
        else
            SetBackLink(session, currentPagePath);

        SetBackLinkAriaLabel();
    }
    
    private static string RegistrationsView(string viewName) => $"~/Views/ReprocessorExporter/Registrations/{viewName}.cshtml";

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