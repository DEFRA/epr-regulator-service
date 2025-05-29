using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Exceptions;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Accreditations;
using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations.AccreditationStatus;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Accreditations;

[FeatureGate(FeatureFlags.ReprocessorExporter)]
[Route(PagePath.ReprocessorExporterAccreditations)]
public class AccreditationStatusController(
    IMapper mapper,
    IValidator<IdAndYearRequest> validator,
    IReprocessorExporterService reprocessorExporterService,
    ISessionManager<JourneySession> sessionManager,
    IConfiguration configuration,
    IOptions<ReprocessorExporterConfig> reprocessorExporterConfig)
    : ReprocessorExporterBaseController(sessionManager, configuration)
{
    [HttpGet]
    [Route(PagePath.FeesDue)]
    public async Task<IActionResult> FeesDue([FromQuery] Guid id, [FromQuery] int year)
    {
        await validator.ValidateAndThrowAsync(new IdAndYearRequest { Id = id, Year = year });

        var session = await GetSession();

        var accreditationStatusSession = await GetOrCreateAccreditationStatusSession(id, session);
        var viewModel = mapper.Map<FeesDueViewModel>(accreditationStatusSession);

        string pagePath = GetPagePath(PagePath.FeesDue, accreditationStatusSession.RegistrationMaterialId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        return View(GetAccreditationStatusView(nameof(FeesDue)), viewModel);
    }

    [HttpGet]
    [Route(PagePath.PaymentCheck)]
    public async Task<IActionResult> PaymentCheck()
    {
        var session = await GetSession();

        var accreditationStatusSession = GetAccreditationStatusSession(session);
        var viewModel = mapper.Map<PaymentCheckViewModel>(accreditationStatusSession);

        string pagePath = GetPagePath(PagePath.PaymentCheck, accreditationStatusSession.RegistrationMaterialId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        return View(GetAccreditationStatusView(nameof(PaymentCheck)), viewModel);
    }

    [HttpPost]
    [Route(PagePath.PaymentCheck)]
    public async Task<IActionResult> PaymentCheck(PaymentCheckViewModel viewModel)
    {
        var session = await GetSession();

        var accreditationStatusSession = GetAccreditationStatusSession(session);

        if (!ModelState.IsValid)
        {
            return HandleInvalidModelState(
                session,
                GetPagePath(PagePath.PaymentCheck, accreditationStatusSession.RegistrationMaterialId),
                accreditationStatusSession,
                viewModel,
                GetAccreditationStatusView(nameof(PaymentCheck))
            );
        }

        accreditationStatusSession.FullPaymentMade = viewModel.FullPaymentMade;

        await SaveSession(session);

        return accreditationStatusSession.FullPaymentMade == true
          ? RedirectToAction("PaymentMethod", "AccreditationStatus")
          : RedirectToAction("QueryMaterialTask", "ManageAccreditations", new { registrationMaterialId = accreditationStatusSession.RegistrationMaterialId, taskName = RegulatorTaskType.CheckAccreditationStatus });//ToDo: this action needs checking

    }

    [HttpGet]
    [Route(PagePath.PaymentMethod)]
    public async Task<IActionResult> PaymentMethod()
    {
        var session = await GetSession();

        var accreditationStatusSession = GetAccreditationStatusSession(session);
        var viewModel = mapper.Map<PaymentMethodViewModel>(accreditationStatusSession);

        string pagePath = GetPagePath(PagePath.PaymentMethod, accreditationStatusSession.RegistrationMaterialId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        return View(GetAccreditationStatusView(nameof(PaymentMethod)), viewModel);
    }

    [HttpPost]
    [Route(PagePath.PaymentMethod)]
    public async Task<IActionResult> PaymentMethod(PaymentMethodViewModel viewModel)
    {
        var session = await GetSession();

        var accreditationStatusSession = GetAccreditationStatusSession(session);

        if (!ModelState.IsValid)
        {
            return HandleInvalidModelState(
                session,
                GetPagePath(PagePath.PaymentMethod, accreditationStatusSession.RegistrationMaterialId),
                accreditationStatusSession,
                viewModel,
                GetAccreditationStatusView(nameof(PaymentMethod))
            );
        }

        accreditationStatusSession.PaymentMethod = viewModel.PaymentMethod;

        await SaveSession(session);

        return RedirectToAction("PaymentDate", "AccreditationStatus");
    }

    [HttpGet]
    [Route(PagePath.PaymentDate)]
    public async Task<IActionResult> PaymentDate()
    {
        var session = await GetSession();

        var accreditationStatusSession = GetAccreditationStatusSession(session);
        var viewModel = mapper.Map<PaymentDateViewModel>(accreditationStatusSession);

        string pagePath = GetPagePath(PagePath.PaymentDate, accreditationStatusSession.RegistrationMaterialId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        return View(GetAccreditationStatusView(nameof(PaymentDate)), viewModel);
    }

    [HttpPost]
    [Route(PagePath.PaymentDate)]
    public async Task<IActionResult> PaymentDate(PaymentDateViewModel viewModel)
    {
        var session = await GetSession();

        var accreditationStatusSession = GetAccreditationStatusSession(session);

        if (!ModelState.IsValid)
        {
            return HandleInvalidModelState(
                session,
                GetPagePath(PagePath.PaymentDate, accreditationStatusSession.RegistrationMaterialId),
                accreditationStatusSession,
                viewModel,
                GetAccreditationStatusView(nameof(PaymentDate))
            );
        }

        accreditationStatusSession.PaymentDate =
            new DateTime(viewModel.Year!.Value, viewModel.Month!.Value, viewModel.Day!.Value);

        await SaveSession(session);

        return RedirectToAction("PaymentReview", "AccreditationStatus");
    }

    [HttpGet]
    [Route(PagePath.PaymentReview)]
    public async Task<IActionResult> PaymentReview()
    {
        var session = await GetSession();

        var accreditationStatusSession = GetAccreditationStatusSession(session);
        var viewModel = mapper.Map<PaymentReviewViewModel>(accreditationStatusSession);

        int determinationWeeks = reprocessorExporterConfig.Value.DeterminationWeeks;
        viewModel.DeterminationWeeks = reprocessorExporterConfig.Value.DeterminationWeeks;

        var dulyMadeDate = CalculateDulyMadeDate(accreditationStatusSession.SubmittedDate, accreditationStatusSession.PaymentDate);
        viewModel.DeterminationDate = CalculateDeterminationDate(determinationWeeks, dulyMadeDate);

        string pagePath = GetPagePath(PagePath.PaymentReview, accreditationStatusSession.RegistrationMaterialId);
        await SaveSessionAndJourney(session, pagePath);
        SetBackLinkInfos(session, pagePath);

        return View(GetAccreditationStatusView(nameof(PaymentReview)), viewModel);
    }

    [HttpPost]
    [Route(PagePath.MarkAsDulyMade)]
    public async Task<IActionResult> MarkAsDulyMade()
    {
        var session = await GetSession();

        var accreditationStatusSession = GetAccreditationStatusSession(session);

        var offlinePaymentRequest = mapper.Map<AccreditationOfflinePaymentRequest>(accreditationStatusSession);
        await reprocessorExporterService.SubmitAccreditationOfflinePaymentAsync(offlinePaymentRequest);

        var dulyMadeRequest = CreateDulyMadeRequest(accreditationStatusSession);
        await reprocessorExporterService.MarkAccreditationAsDulyMadeAsync(accreditationStatusSession.RegistrationMaterialId, dulyMadeRequest);

        return RedirectToAction("Index", "ManageAccreditations", new { id = accreditationStatusSession.RegistrationId, year = accreditationStatusSession.Year });
    }

    private AccreditationMarkAsDulyMadeRequest CreateDulyMadeRequest(AccreditationStatusSession accreditationStatusSession)
    {
        var dulyMadeDate = CalculateDulyMadeDate(accreditationStatusSession.SubmittedDate, accreditationStatusSession.PaymentDate);

        var dulyMadeRequest = new AccreditationMarkAsDulyMadeRequest
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

    private static string GetPagePath(string pagePath, Guid registrationMaterialId) =>
        $"{pagePath}?registrationMaterialId={registrationMaterialId}";

    private static AccreditationStatusSession GetAccreditationStatusSession(JourneySession session)
    {
        if (session.ReprocessorExporterSession.AccreditationStatusSession == null)
        {
            throw new SessionException("AccreditationStatusSession does not exist");
        }

        return session.ReprocessorExporterSession.AccreditationStatusSession;
    }

    private async Task<AccreditationStatusSession> GetOrCreateAccreditationStatusSession(Guid accreditationMaterialId, JourneySession session)
    {
        if (session.ReprocessorExporterSession.AccreditationStatusSession == null)
        {
            var accreditationMaterial = await reprocessorExporterService.GetPaymentFeesByAccreditationMaterialIdAsync(accreditationMaterialId);
            var accreditationStatusSession = mapper.Map<AccreditationStatusSession>(accreditationMaterial);

            session.ReprocessorExporterSession.AccreditationStatusSession = accreditationStatusSession;
        }

        return session.ReprocessorExporterSession.AccreditationStatusSession;
    }

    private IActionResult HandleInvalidModelState<T>(JourneySession session, string pagePath, AccreditationStatusSession accreditationStatusSession, T viewModel, string viewName)
    {
        SetBackLinkInfos(session, pagePath);

        mapper.Map(accreditationStatusSession, viewModel);

        return View(viewName, viewModel);
    }

    protected static string GetAccreditationStatusView(string viewName) => $"~/Views/ReprocessorExporter/Accreditations/AccreditationStatus/{viewName}.cshtml";
}