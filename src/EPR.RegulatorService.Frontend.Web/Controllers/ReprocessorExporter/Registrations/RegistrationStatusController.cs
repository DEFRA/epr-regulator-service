using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Exceptions;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.RegistrationStatus;

using FluentValidation;

using Microsoft.FeatureManagement.Mvc;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Options;

namespace EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations;

[FeatureGate(FeatureFlags.ReprocessorExporter)]
[Route(PagePath.ReprocessorExporterRegistrations)]
public class RegistrationStatusController(
    IMapper mapper,
    IValidator<IdRequest> validator,
    IReprocessorExporterService reprocessorExporterService,
    ISessionManager<JourneySession> sessionManager,
    IConfiguration configuration,
    IOptions<ReprocessorExporterConfig> reprocessorExporterConfig)
    : ReprocessorExporterBaseController(sessionManager, configuration)
{
    [HttpGet]
    [Route(PagePath.FeesDue)]
    public async Task<IActionResult> FeesDue(int registrationMaterialId)
    {
        await validator.ValidateAndThrowAsync(new IdRequest { Id = registrationMaterialId });

        var session = await GetSession();
        
        var registrationStatusSession = await GetOrCreateRegistrationStatusSession(registrationMaterialId, session);
        var viewModel = mapper.Map<FeesDueViewModel>(registrationStatusSession);

        string pagePath = GetPagePath(PagePath.FeesDue, registrationStatusSession.RegistrationMaterialId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        return View(GetRegistrationStatusView(nameof(FeesDue)), viewModel);
    }

    [HttpGet]
    [Route(PagePath.PaymentCheck)]
    public async Task<IActionResult> PaymentCheck()
    {
        var session = await GetSession();

        var registrationStatusSession = GetRegistrationStatusSession(session);
        var viewModel = mapper.Map<PaymentCheckViewModel>(registrationStatusSession);

        string pagePath = GetPagePath(PagePath.PaymentCheck, registrationStatusSession.RegistrationMaterialId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        return View(GetRegistrationStatusView(nameof(PaymentCheck)), viewModel);
    }

    [HttpPost]
    [Route(PagePath.PaymentCheck)]
    public async Task<IActionResult> PaymentCheck(PaymentCheckViewModel viewModel)
    {
        var session = await GetSession();

        var registrationStatusSession = GetRegistrationStatusSession(session);

        if (!ModelState.IsValid)
        {
            return HandleInvalidModelState(
                session,
                GetPagePath(PagePath.PaymentCheck, registrationStatusSession.RegistrationMaterialId),
                registrationStatusSession,
                viewModel,
                GetRegistrationStatusView(nameof(PaymentCheck))
            );
        }

        registrationStatusSession.FullPaymentMade = viewModel.FullPaymentMade;

        await SaveSession(session);

        return RedirectToAction(PagePath.PaymentMethod, PagePath.ReprocessorExporterRegistrations);
    }

    [HttpGet]
    [Route(PagePath.PaymentMethod)]
    public async Task<IActionResult> PaymentMethod()
    {
        var session = await GetSession();

        var registrationStatusSession = GetRegistrationStatusSession(session);
        var viewModel = mapper.Map<PaymentMethodViewModel>(registrationStatusSession);

        string pagePath = GetPagePath(PagePath.PaymentMethod, registrationStatusSession.RegistrationMaterialId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        return View(GetRegistrationStatusView(nameof(PaymentMethod)), viewModel);
    }

    [HttpPost]
    [Route(PagePath.PaymentMethod)]
    public async Task<IActionResult> PaymentMethod(PaymentMethodViewModel viewModel)
    {
        var session = await GetSession();

        var registrationStatusSession = GetRegistrationStatusSession(session);

        if (!ModelState.IsValid)
        {
            return HandleInvalidModelState(
                session,
                GetPagePath(PagePath.PaymentMethod, registrationStatusSession.RegistrationMaterialId),
                registrationStatusSession,
                viewModel,
                GetRegistrationStatusView(nameof(PaymentMethod))
            );
        }

        registrationStatusSession.PaymentMethod = viewModel.PaymentMethod;

        await SaveSession(session);

        return RedirectToAction(PagePath.PaymentDate, PagePath.ReprocessorExporterRegistrations);
    }

    [HttpGet]
    [Route(PagePath.PaymentDate)]
    public async Task<IActionResult> PaymentDate()
    {
        var session = await GetSession();

        var registrationStatusSession = GetRegistrationStatusSession(session);
        var viewModel = mapper.Map<PaymentDateViewModel>(registrationStatusSession);

        string pagePath = GetPagePath(PagePath.PaymentDate, registrationStatusSession.RegistrationMaterialId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        return View(GetRegistrationStatusView(nameof(PaymentDate)), viewModel);
    }

    [HttpPost]
    [Route(PagePath.PaymentDate)]
    public async Task<IActionResult> PaymentDate(PaymentDateViewModel viewModel)
    {
        var session = await GetSession();

        var registrationStatusSession = GetRegistrationStatusSession(session);

        if (!ModelState.IsValid)
        {
            return HandleInvalidModelState(
                session,
                GetPagePath(PagePath.PaymentDate, registrationStatusSession.RegistrationMaterialId),
                registrationStatusSession,
                viewModel,
                GetRegistrationStatusView(nameof(PaymentDate))
            );
        }

        registrationStatusSession.PaymentDate =
            new DateTime(viewModel.Year!.Value, viewModel.Month!.Value, viewModel.Day!.Value);

        await SaveSession(session);

        return RedirectToAction(PagePath.PaymentReview, PagePath.ReprocessorExporterRegistrations);
    }

    [HttpGet]
    [Route(PagePath.PaymentReview)]
    public async Task<IActionResult> PaymentReview()
    {
        var session = await GetSession();

        var registrationStatusSession = GetRegistrationStatusSession(session);
        var viewModel = mapper.Map<PaymentReviewViewModel>(registrationStatusSession);

        int determinationWeeks = reprocessorExporterConfig.Value.DeterminationWeeks;
        viewModel.DeterminationWeeks = reprocessorExporterConfig.Value.DeterminationWeeks;

        var dulyMadeDate = CalculateDulyMadeDate(registrationStatusSession.SubmittedDate, registrationStatusSession.PaymentDate);
        viewModel.DeterminationDate = CalculateDeterminationDate(determinationWeeks, dulyMadeDate);

        string pagePath = GetPagePath(PagePath.PaymentReview, registrationStatusSession.RegistrationMaterialId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        return View(GetRegistrationStatusView(nameof(PaymentReview)), viewModel);
    }
    
    [HttpGet]
    [Route(PagePath.MarkAsDulyMade)]
    public async Task<IActionResult> MarkAsDulyMade()
    {
        var session = await GetSession();

        var registrationStatusSession = GetRegistrationStatusSession(session);

        var offlinePaymentRequest = mapper.Map<OfflinePaymentRequest>(registrationStatusSession);
        await reprocessorExporterService.SubmitOfflinePaymentAsync(offlinePaymentRequest);

        var dulyMadeRequest = CreateDulyMadeRequest(registrationStatusSession);
        await reprocessorExporterService.MarkAsDulyMadeAsync(registrationStatusSession.RegistrationMaterialId, dulyMadeRequest);

        return RedirectToAction("Index", "ManageRegistrations", new { id = registrationStatusSession.RegistrationId });
    }

    private MarkAsDulyMadeRequest CreateDulyMadeRequest(RegistrationStatusSession registrationStatusSession)
    {
        var dulyMadeDate = CalculateDulyMadeDate(registrationStatusSession.SubmittedDate, registrationStatusSession.PaymentDate);

        var dulyMadeRequest = new MarkAsDulyMadeRequest
        {
            DulyMadeDate = dulyMadeDate,
            DeterminationDate =
                CalculateDeterminationDate(reprocessorExporterConfig.Value.DeterminationWeeks, dulyMadeDate)
        };
        return dulyMadeRequest;
    }

    private static DateTime CalculateDulyMadeDate(DateTime submittedDate, DateTime? paymentDate) =>
        paymentDate.HasValue && paymentDate.Value > submittedDate
            ? paymentDate.Value
            : submittedDate;

    private static DateTime CalculateDeterminationDate(int determinationWeeks, DateTime dulyMadeDate) => dulyMadeDate.AddDays(determinationWeeks * 7);

    private static string GetPagePath(string pagePath, int registrationMaterialId) =>
        $"{pagePath}?registrationMaterialId={registrationMaterialId}";

    private static RegistrationStatusSession GetRegistrationStatusSession(JourneySession session)
    {
        if (session.ReprocessorExporterSession.RegistrationStatusSession == null)
        {
            throw new SessionException("RegistrationStatusSession does not exist");
        }

        return session.ReprocessorExporterSession.RegistrationStatusSession;
    }

    private async Task<RegistrationStatusSession> GetOrCreateRegistrationStatusSession(int registrationMaterialId, JourneySession session)
    {
        if (session.ReprocessorExporterSession.RegistrationStatusSession == null)
        {
            var registrationMaterial = await reprocessorExporterService.GetPaymentFeesByRegistrationMaterialIdAsync(registrationMaterialId);
            var registrationStatusSession = mapper.Map<RegistrationStatusSession>(registrationMaterial);

            session.ReprocessorExporterSession.RegistrationStatusSession = registrationStatusSession;
        }

        return session.ReprocessorExporterSession.RegistrationStatusSession;
    }

    private IActionResult HandleInvalidModelState<T>(JourneySession session, string pagePath, RegistrationStatusSession registrationStatusSession, T viewModel, string viewName)
    {
        SetBackLinkInfos(session, pagePath);

        mapper.Map(registrationStatusSession, viewModel);

        return View(viewName, viewModel);
    }

    protected static string GetRegistrationStatusView(string viewName) => $"~/Views/ReprocessorExporter/Registrations/RegistrationStatus/{viewName}.cshtml";
}