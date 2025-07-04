using AspNetCoreGeneratedDocument;

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
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;
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
    : AccreditationBaseController(sessionManager, configuration)
{
    [HttpGet]
    [Route(PagePath.FeesDue)]
    public async Task<IActionResult> FeesDue([FromQuery] Guid id, [FromQuery] int year)
    {
        await validator.ValidateAndThrowAsync(new IdAndYearRequest { Id = id, Year = year });

        var session = await GetSession();

        var accreditationStatusSession = await GetOrCreateAccreditationStatusSession(id, session);
        var viewModel = mapper.Map<FeesDueViewModel>(accreditationStatusSession);

        accreditationStatusSession.Year = year;

        string pagePath = GetPagePath(PagePath.FeesDue, accreditationStatusSession.AccreditationId, year);
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

        string pagePath = GetPagePath(PagePath.PaymentCheck, accreditationStatusSession.AccreditationId, accreditationStatusSession.Year);
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
                GetPagePath(PagePath.PaymentCheck, accreditationStatusSession.AccreditationId, accreditationStatusSession.Year),
                accreditationStatusSession,
                viewModel,
                GetAccreditationStatusView(nameof(PaymentCheck))
            );
        }

        accreditationStatusSession.FullPaymentMade = viewModel.FullPaymentMade;

        await SaveSession(session);

        return accreditationStatusSession.FullPaymentMade == true
          ? RedirectToAction("PaymentMethod", "AccreditationStatus")
          : RedirectToAction("QueryAccreditationTask", "AccreditationStatus", new { accreditationId = accreditationStatusSession.AccreditationId, taskName = RegulatorTaskType.DulyMade });

    }

    [HttpGet]
    [Route(PagePath.PaymentMethod)]
    public async Task<IActionResult> PaymentMethod()
    {
        var session = await GetSession();

        var accreditationStatusSession = GetAccreditationStatusSession(session);
        var viewModel = mapper.Map<PaymentMethodViewModel>(accreditationStatusSession);

        string pagePath = GetPagePath(PagePath.PaymentMethod, accreditationStatusSession.AccreditationId, accreditationStatusSession.Year);
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
                GetPagePath(PagePath.PaymentMethod, accreditationStatusSession.AccreditationId, accreditationStatusSession.Year),
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

        string pagePath = GetPagePath(PagePath.PaymentDate, accreditationStatusSession.AccreditationId, accreditationStatusSession.Year);
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
                GetPagePath(PagePath.PaymentDate, accreditationStatusSession.AccreditationId, accreditationStatusSession.Year),
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

        string pagePath = GetPagePath(PagePath.PaymentReview, accreditationStatusSession.AccreditationId, accreditationStatusSession.Year);
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
        await reprocessorExporterService.MarkAccreditationAsDulyMadeAsync(accreditationStatusSession.AccreditationId, dulyMadeRequest);

        return RedirectToAction("Index", "ManageAccreditations", new { id = session.ReprocessorExporterSession.RegistrationId, year = accreditationStatusSession.Year });
    }

    [HttpGet]
    [Route(PagePath.QueryAccreditationTask)]
    public async Task<IActionResult> QueryAccreditationTask(Guid accreditationId, RegulatorTaskType taskName)
    {
        var session = await GetSession();

        await SaveSessionAndJourney(session, PagePath.QueryAccreditationTask);
        SetBackLinkInfos(session, PagePath.QueryAccreditationTask);

        var queryAccreditationTaskViewModel = new QueryAccreditationTaskViewModel
        {
            AccreditationId = accreditationId,
            TaskName = taskName
        };

        return View(GetAccreditationStatusView(nameof(QueryAccreditationTask)), queryAccreditationTaskViewModel);
    }

    [HttpPost]
    [Route(PagePath.QueryAccreditationTask)]
    public async Task<IActionResult> QueryAccreditationTask(QueryAccreditationTaskViewModel queryAccreditationTaskViewModel)
    {
        var session = await GetSession();

        if (!ModelState.IsValid)
        {
            SetBackLinkInfos(session, PagePath.QueryMaterialTask);
            return View(GetAccreditationStatusView(nameof(QueryAccreditationTask)), queryAccreditationTaskViewModel);
        }

        var updateAccreditationTaskStatusRequest = new UpdateAccreditationTaskStatusRequest
        {
            TaskName = queryAccreditationTaskViewModel.TaskName.ToString(),
            AccreditationId = queryAccreditationTaskViewModel.AccreditationId,
            Status = RegulatorTaskStatus.Queried.ToString(),
            Comments = queryAccreditationTaskViewModel.Comments
        };

        await reprocessorExporterService.UpdateRegulatorAccreditationTaskStatusAsync(updateAccreditationTaskStatusRequest);

        return RedirectToAction("Index", "ManageAccreditations", new
        {
            id = session.ReprocessorExporterSession.RegistrationId,
            year = session.ReprocessorExporterSession.AccreditationStatusSession.Year
        });
    }

    [HttpGet]
    [Route(PagePath.AccreditationBusinessPlan)]
    public async Task<IActionResult> AccreditationBusinessPlan(Guid accreditationId, int year)
    {
        var session = await GetSession();
        SetBackLinkInfos(session, PagePath.AccreditationBusinessPlan);
        InitialiseAccreditationStatusSessionIfNotExists(session, accreditationId, year);

        await SaveSessionAndJourney(session, PagePath.QueryAccreditationTask);

        var accreditationBusinessPlan = await reprocessorExporterService.GetAccreditionBusinessPlanByIdAsync(accreditationId);
        var accreditationBusinessPlanViewModel = mapper.Map<AccreditationBusinessPlanViewModel>(accreditationBusinessPlan);

        return View(GetAccreditationStatusView(nameof(AccreditationBusinessPlan)), accreditationBusinessPlanViewModel);
    }


    [HttpPost]
    [Route(PagePath.AccreditationBusinessPlan)]
    public async Task<IActionResult> CompleteAccreditationBusinessPlan(Guid accreditationId)
    {
        var session = await GetSession();

        var updateAccreditationTaskStatusRequest = new UpdateAccreditationTaskStatusRequest
        {
            TaskName = RegulatorTaskType.BusinessPlan.ToString(),
            AccreditationId = accreditationId,
            Status = RegulatorTaskStatus.Completed.ToString(),
            Comments = string.Empty
        };

        await reprocessorExporterService.UpdateRegulatorAccreditationTaskStatusAsync(updateAccreditationTaskStatusRequest);

        return RedirectToAction("Index", "ManageAccreditations", new
        {
            id = session.ReprocessorExporterSession.RegistrationId,
            year = session.ReprocessorExporterSession.AccreditationStatusSession.Year
        });

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

    private static string GetPagePath(string pagePath, Guid accreditationId, int? year) =>
        year == null ?
            $"{pagePath}?id={accreditationId}" :
        $"{pagePath}?id={accreditationId}&year={year}";


    private static AccreditationStatusSession GetAccreditationStatusSession(JourneySession session)
    {
        if (session.ReprocessorExporterSession.AccreditationStatusSession == null)
        {
            throw new SessionException("AccreditationStatusSession does not exist");
        }

        return session.ReprocessorExporterSession.AccreditationStatusSession;
    }

    private async Task<AccreditationStatusSession> GetOrCreateAccreditationStatusSession(Guid accreditationId, JourneySession session)
    {
        if (session.ReprocessorExporterSession.AccreditationStatusSession == null)
        {
            var accreditationMaterial = await reprocessorExporterService.GetPaymentFeesByAccreditationIdAsync(accreditationId);
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