using System.Globalization;
using System.Net;

using EPR.Common.Authorization.Constants;
using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Extensions;
using EPR.RegulatorService.Frontend.Core.Models.FileDownload;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Web.ViewModels.Registrations;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement.Mvc;


namespace EPR.RegulatorService.Frontend.Web.Controllers.RegistrationSubmissions;

[FeatureGate(FeatureFlags.ManageRegistrationSubmissions)]
[Authorize(Policy = PolicyConstants.RegulatorBasicPolicy)]
public partial class RegistrationSubmissionsController(
                IFacadeService facade,
                IPaymentFacadeService paymentFacade,
                ISessionManager<JourneySession> sessionManager,
                ILogger<RegistrationSubmissionsController> logger,
                IConfiguration configuration,
                IOptions<ExternalUrlsOptions> externalUrlsOptions,
                IOptions<SubmissionFiltersConfig> submissionFiltersConfig
             ) : Controller
{
    private readonly string _pathBase = configuration.GetValue<string>(ConfigKeys.PathBase);
    private readonly ExternalUrlsOptions _externalUrlsOptions = externalUrlsOptions.Value;
    private readonly SubmissionFiltersConfig _submissionFiltersConfig = submissionFiltersConfig.Value;
    private readonly ISessionManager<JourneySession> _sessionManager = sessionManager;
    private readonly IFacadeService _facadeService = facade;
    private readonly IPaymentFacadeService _paymentFacadeService = paymentFacade;
    private JourneySession _currentSession;
    private static readonly Action<ILogger, string, Exception?> _logControllerError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(1001, nameof(RegistrationSubmissionsController)),
            "An error occurred while processing a message: {ErrorMessage}");

    [HttpGet]
    [Consumes("application/json")]
    [Route(PagePath.RegistrationSubmissionsRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrationSubmissions(int? pageNumber)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new JourneySession();

        int nationId = _currentSession.UserData.Organisations[0].NationId ?? 0;

        InitialiseOrContinuePaging(_currentSession.RegulatorRegistrationSubmissionSession, pageNumber);

        ViewBag.PowerBiLogin = _externalUrlsOptions.PowerBiLogin;

        SetBacklinkToHome();

        var viewModel = InitialiseOrCreateViewModel(
            _currentSession.RegulatorRegistrationSubmissionSession,
            nationId);

        await SaveSessionAndJourney(_currentSession.RegulatorRegistrationSubmissionSession, PagePath.RegistrationSubmissionsRoute, PagePath.RegistrationSubmissionsRoute);

        return View(viewModel);
    }

    [HttpPost]
    [Route(PagePath.RegistrationSubmissionsRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrationSubmissions([FromForm] RegistrationSubmissionsFilterViewModel? filters = null,
                                                             [FromForm] string? filterType = null)
    {
        try
        {
            _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

            if (ReturnIfAppropriate(filters, filterType) is IActionResult response)
            {
                return response;
            }

            ClearFilters(_currentSession.RegulatorRegistrationSubmissionSession,
                               filters,
                               filterType == FilterActions.ClearFilters);
            UpdateRegistrationSubmissionFiltersInSession(_currentSession.RegulatorRegistrationSubmissionSession,
                               filters,
                               filterType == FilterActions.SubmitFilters);
            await SaveSessionAndJourney(_currentSession.RegulatorRegistrationSubmissionSession, PagePath.RegistrationSubmissionsRoute, PagePath.RegistrationSubmissionsRoute);
        }
        catch (Exception ex)
        {
            _logControllerError.Invoke(logger, $"Exception received processing POST to {nameof(RegistrationSubmissionsController)}.{nameof(RegistrationSubmissions)}", ex);
            return RedirectToAction(PagePath.Error, "Error");
        }

        return RedirectToAction(PagePath.RegistrationSubmissionsAction);
    }

    [HttpGet]
    [Route(PagePath.RegistrationSubmissionDetails + "/{submissionId:guid}", Name = "SubmissionDetails")]
    public async Task<IActionResult> RegistrationSubmissionDetails(Guid? submissionId)
    {
        try
        {
            _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

            RegistrationSubmissionDetailsViewModel model;
            if (!GetOrRejectProvidedSubmissionId(submissionId.Value, out model))
            {
                var details = await FetchFromSessionOrFacadeAsync(submissionId.Value, _facadeService.GetRegistrationSubmissionDetails);
                model = details != null ? (RegistrationSubmissionDetailsViewModel)details : null;
            }

            if (model is null)
            {
                return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
            }

            GeneratePowerBILink(model);
            SetBackLink(PagePath.RegistrationSubmissionsRoute);
            ViewBag.SubmissionId = model.SubmissionId;

            await SaveSessionAndJourney(_currentSession.RegulatorRegistrationSubmissionSession, PagePath.RegistrationSubmissionsRoute, PagePath.RegistrationSubmissionsRoute);

            return View(nameof(RegistrationSubmissionDetails), model);
        }
        catch (HttpRequestException hex)
        {
            if (hex.StatusCode == HttpStatusCode.NotFound)
            {
                _logControllerError.Invoke(logger, $"Exception received processing GET to {nameof(RegistrationSubmissionsController)}.{nameof(RegistrationSubmissions)}", hex);
                return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
            }
            return RedirectToAction(PagePath.Error, "Error");
        }
        catch (Exception ex)
        {
            _logControllerError.Invoke(logger, $"Exception received processing GET to {nameof(RegistrationSubmissionsController)}.{nameof(RegistrationSubmissions)}", ex);
            return RedirectToAction(PagePath.Error, "Error");
        }
    }

    [HttpPost]
    [Route(PagePath.RegistrationSubmissionDetails + "/{submissionId:guid}", Name = "SubmitPaymentInfo")]
    public async Task<IActionResult> SubmitOfflinePayment([FromForm] PaymentDetailsViewModel paymentDetailsViewModel, [FromRoute] Guid? submissionId)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);
        var details = await FetchFromSessionOrFacadeAsync(submissionId.Value, _facadeService.GetRegistrationSubmissionDetails);
        RegistrationSubmissionDetailsViewModel model = details != null ? (RegistrationSubmissionDetailsViewModel)details : null;

        if (model is null)
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        if (!ModelState.IsValid)
        {
            return View(nameof(RegistrationSubmissionDetails), model);
        }

        if (decimal.TryParse(paymentDetailsViewModel.OfflinePayment, NumberStyles.Currency, CultureInfo.InvariantCulture, out decimal parsedValue))
        {
            paymentDetailsViewModel.OfflinePayment = parsedValue.ToString("F2", CultureInfo.InvariantCulture);
        }
        TempData["OfflinePaymentAmount"] = paymentDetailsViewModel.OfflinePayment;

        await SaveSessionAndJourney(
            _currentSession.RegulatorRegistrationSubmissionSession,
            PagePath.RegistrationSubmissionsRoute,
            PagePath.RegistrationSubmissionsRoute);

        return Redirect(Url.RouteUrl("ConfirmOfflinePaymentSubmission", new { submissionId }));
    }

    [HttpGet]
    [Route(PagePath.GrantRegistrationSubmission + "/{submissionId:guid}", Name = "GrantRegistrationSubmission")]
    public async Task<IActionResult> GrantRegistrationSubmission(Guid? submissionId)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedSubmissionId(submissionId, out var existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        SetBackLink($"{PagePath.RegistrationSubmissionDetails}/{existingModel.SubmissionId}");

        var model = new GrantRegistrationSubmissionViewModel
        {
            SubmissionId = existingModel.SubmissionId,
            IsResubmission = existingModel.IsResubmission
        };

        ViewBag.BackToAllSubmissionsUrl = Url.Action("RegistrationSubmissions");

        return View(nameof(GrantRegistrationSubmission), model);
    }

    [HttpPost]
    [Route(PagePath.GrantRegistrationSubmission + "/{submissionId:guid}", Name = "GrantRegistrationSubmission")]
    public async Task<IActionResult> GrantRegistrationSubmission(GrantRegistrationSubmissionViewModel model)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedSubmissionId(model.SubmissionId, out var existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        if (!ModelState.IsValid)
        {
            SetBackLink($"{PagePath.RegistrationSubmissionDetails}/{existingModel.SubmissionId}");
            ViewBag.BackToAllSubmissionsUrl = Url.Action("RegistrationSubmissions");
            return View(nameof(GrantRegistrationSubmission), model);
        }

        if (!model.IsGrantRegistrationConfirmed.Value)
        {
            return RedirectToRoute("SubmissionDetails", new { existingModel.SubmissionId });
        }

        try
        {
            var regulatorDecisionRequest = GetDecisionRequest(existingModel, Core.Enums.RegistrationSubmissionStatus.Granted);
            var status = await _facadeService.SubmitRegulatorRegistrationDecisionAsync(regulatorDecisionRequest);

            _currentSession.RegulatorRegistrationSubmissionSession.SelectedRegistrations.Remove(existingModel.SubmissionId);
            _currentSession.RegulatorRegistrationSubmissionSession.OrganisationDetailsChangeHistory.Remove(existingModel.SubmissionId);

            SaveSession(_currentSession);

            return status == Core.Models.EndpointResponseStatus.Success
                  ? RedirectToRoute("SubmissionDetails", new { existingModel.SubmissionId })
                  : RedirectToRoute("ServiceNotAvailable", new { backLink = $"{PagePath.RegistrationSubmissionDetails}/{existingModel.SubmissionId}" });
        }
        catch (Exception ex)
        {
            _logControllerError.Invoke(logger, $"Exception received while granting submission {nameof(RegistrationSubmissionsController)}.{nameof(GrantRegistrationSubmission)}", ex);
            return RedirectToRoute("ServiceNotAvailable", new { backLink = $"{PagePath.RegistrationSubmissionDetails}/{existingModel.SubmissionId}" });
        }
    }

    [HttpGet]
    [Route(PagePath.QueryRegistrationSubmission + "/{submissionId:guid}")]
    public async Task<IActionResult> QueryRegistrationSubmission(Guid? submissionId)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedSubmissionId(submissionId, out RegistrationSubmissionDetailsViewModel existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        SetBackLink(Url.RouteUrl("SubmissionDetails", new { submissionId }), false);

        var model = new QueryRegistrationSubmissionViewModel
        {
            SubmissionId = submissionId.Value
        };

        ViewBag.BackToAllSubmissionsUrl = Url.Action("RegistrationSubmissions");

        return View(nameof(QueryRegistrationSubmission), model);
    }

    [HttpPost]
    [Route(PagePath.QueryRegistrationSubmission + "/{submissionId:guid}")]
    public async Task<IActionResult> QueryRegistrationSubmission(QueryRegistrationSubmissionViewModel model)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedSubmissionId(model.SubmissionId, out var existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        if (!ModelState.IsValid)
        {
            SetBackLink(Url.RouteUrl("SubmissionDetails", new { model.SubmissionId }), false);
            ViewBag.BackToAllSubmissionsUrl = Url.Action("RegistrationSubmissions");
            return View(nameof(QueryRegistrationSubmission), model);
        }

        try
        {
            var regulatorDecisionRequest = GetDecisionRequest(existingModel, Core.Enums.RegistrationSubmissionStatus.Queried);
            regulatorDecisionRequest.Comments = model.Query;

            var status = await _facadeService.SubmitRegulatorRegistrationDecisionAsync(regulatorDecisionRequest);

            await UpdateOrganisationDetailsChangeHistoryAsync(existingModel, status, regulatorDecisionRequest);

            return status == EndpointResponseStatus.Success
                ? RedirectToAction(PagePath.RegistrationSubmissionsAction)
                : RedirectToRoute("ServiceNotAvailable",
                new
                {
                    backLink = $"{PagePath.RegistrationSubmissionDetails}/{existingModel.SubmissionId}"
                });
        }
        catch (Exception ex)
        {
            _logControllerError.Invoke(
                logger,
                $"Exception received while querying submission" +
                $"{nameof(RegistrationSubmissionsController)}.{nameof(QueryRegistrationSubmission)}", ex);

            return RedirectToRoute(
                "ServiceNotAvailable",
                new
                {
                    backLink = $"{PagePath.RegistrationSubmissionDetails}/{existingModel.SubmissionId}"
                });
        }
    }

    [HttpGet]
    [Route(PagePath.RejectRegistrationSubmission + "/{submissionId:guid}", Name = "RejectRegistrationSubmission")]
    public async Task<IActionResult> RejectRegistrationSubmission(Guid? submissionId)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedSubmissionId(submissionId, out RegistrationSubmissionDetailsViewModel existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }
        SetBackLink($"{PagePath.RegistrationSubmissionDetails}/{submissionId}");

        var model = new RejectRegistrationSubmissionViewModel
        {
            SubmissionId = submissionId.Value,
            IsResubmission = existingModel.IsResubmission
        };

        ViewBag.BackToAllSubmissionsUrl = Url.Action("RegistrationSubmissions");

        return View(nameof(RejectRegistrationSubmission), model);
    }

    [HttpPost]
    [Route(PagePath.RejectRegistrationSubmission + "/{submissionId:guid}")]
    public async Task<IActionResult> RejectRegistrationSubmission(RejectRegistrationSubmissionViewModel model)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedSubmissionId(model.SubmissionId, out var existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        if (!ModelState.IsValid)
        {
            SetBackLink(Url.RouteUrl("SubmissionDetails", new { model.SubmissionId }), false);
            ViewBag.BackToAllSubmissionsUrl = Url.Action("RegistrationSubmissions");
            return View(nameof(RejectRegistrationSubmission), model);
        }

        _currentSession.RegulatorRegistrationSubmissionSession.SelectedRegistrations[existingModel.SubmissionId].RejectReason = model.RejectReason;
        existingModel.RegulatorComments = model.RejectReason;
        existingModel.RejectReason = model.RejectReason;

        if (_currentSession!.RegulatorRegistrationSubmissionSession.OrganisationDetailsChangeHistory.TryGetValue(existingModel.SubmissionId, out var organisationDetailsChangeHistory))
        {
            organisationDetailsChangeHistory.RejectReason = model.RejectReason;
            organisationDetailsChangeHistory.RegulatorComments = model.RejectReason;
            organisationDetailsChangeHistory.RegulatorDecisionDate = DateTime.UtcNow;
            organisationDetailsChangeHistory.SubmissionDetails.DecisionDate = DateTime.UtcNow;
        }

        SaveSessionAndJourney(
            _currentSession.RegulatorRegistrationSubmissionSession,
            PagePath.RejectRegistrationSubmission,
            PagePath.ConfirmRegistrationRefusal);

        return existingModel.IsResubmission
            ? await SubmitRegulatorRejectDecisionAsync(existingModel)
            : RedirectToRoute("ConfirmRegistrationRefusal", new { submissionId = existingModel.SubmissionId });
    }

    [HttpGet]
    [Route(PagePath.CancelRegistrationSubmission + "/{submissionId:guid}", Name = "CancelRegistrationSubmission")]
    public async Task<IActionResult> CancelRegistrationSubmission(Guid? submissionId)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedSubmissionId(submissionId, out var existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        SetBackLink($"{PagePath.RegistrationSubmissionDetails}/{submissionId}");

        var model = new CancelRegistrationSubmissionViewModel
        {
            SubmissionId = submissionId.Value
        };

        ViewBag.BackToAllSubmissionsUrl = Url.Action("RegistrationSubmissions");

        return View(nameof(CancelRegistrationSubmission), model);
    }

    [HttpPost]
    [Route(PagePath.CancelRegistrationSubmission + "/{submissionId:guid}")]
    public async Task<IActionResult> CancelRegistrationSubmission(CancelRegistrationSubmissionViewModel model)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedSubmissionId(model.SubmissionId, out var existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        if (!ModelState.IsValid)
        {
            SetBackLink(Url.RouteUrl("SubmissionDetails", new { model.SubmissionId }), false);
            ViewBag.BackToAllSubmissionsUrl = Url.Action("RegistrationSubmissions");
            return View(nameof(CancelRegistrationSubmission), model);
        }

        _currentSession.RegulatorRegistrationSubmissionSession.SelectedRegistrations[existingModel.SubmissionId].CancellationReason = model.CancellationReason;

        SaveSessionAndJourney(
            _currentSession.RegulatorRegistrationSubmissionSession,
            PagePath.CancelRegistrationSubmission,
            PagePath.CancelDateRegistrationSubmission);

        return RedirectToRoute("CancelDateRegistrationSubmission", new { submissionId = existingModel.SubmissionId });
    }

    [HttpGet]
    [Route(PagePath.ConfirmOfflinePaymentSubmission + "/{submissionId:guid}", Name = "ConfirmOfflinePaymentSubmission")]
    public async Task<IActionResult> ConfirmOfflinePaymentSubmission(Guid? submissionId)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);
        var details = await FetchFromSessionOrFacadeAsync(submissionId.Value, _facadeService.GetRegistrationSubmissionDetails);
        RegistrationSubmissionDetailsViewModel model = details != null ? (RegistrationSubmissionDetailsViewModel)details : null;
        if (model is null)
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        string offlinePayment = TempData.Peek("OfflinePaymentAmount").ToString();
        if (string.IsNullOrWhiteSpace(offlinePayment))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        SetBackLink(Url.RouteUrl("SubmissionDetails", new { submissionId }), false);

        var confirmModel = new ConfirmOfflinePaymentSubmissionViewModel
        {
            SubmissionId = submissionId,
            OfflinePaymentAmount = offlinePayment
        };

        return View(nameof(ConfirmOfflinePaymentSubmission), confirmModel);
    }

    [HttpPost]
    [Route(PagePath.ConfirmOfflinePaymentSubmission + "/{submissionId:guid}", Name = "ConfirmOfflinePaymentSubmission")]
    public async Task<IActionResult> ConfirmOfflinePaymentSubmission(ConfirmOfflinePaymentSubmissionViewModel model)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);
        var details = await FetchFromSessionOrFacadeAsync(model.SubmissionId.Value, _facadeService.GetRegistrationSubmissionDetails);
        RegistrationSubmissionDetailsViewModel regSubmissionDetails = details != null ? (RegistrationSubmissionDetailsViewModel)details : null;
        if (regSubmissionDetails is null)
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        if (!ModelState.IsValid)
        {
            SetBackLink(Url.RouteUrl("SubmissionDetails", new { model.SubmissionId }), false);
            return View(nameof(ConfirmOfflinePaymentSubmission), model);
        }
        else if (!(bool)model.IsOfflinePaymentConfirmed)
        {
            return RedirectToRoute("SubmissionDetails", new { regSubmissionDetails.SubmissionId });
        }

        TempData.Remove("OfflinePaymentAmount");
        return string.IsNullOrWhiteSpace(model.OfflinePaymentAmount)
            ? RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions")
            : await ProcessOfflinePaymentAsync(regSubmissionDetails, model.OfflinePaymentAmount);
    }

    [HttpGet]
    [Route(PagePath.CancellationConfirmation + "/{submissionId:guid}", Name = "CancellationConfirmation")]
    public async Task<IActionResult> CancellationConfirmation(Guid? submissionId)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedSubmissionId(submissionId, out var existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        if (string.IsNullOrEmpty(existingModel.OrganisationName))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        var model = new CancellationConfirmationViewModel
        {
            SubmissionId = existingModel.SubmissionId,
            OrganisationName = existingModel.OrganisationName
        };

        ViewBag.BackToAllSubmissionsUrl = Url.Action("RegistrationSubmissions");

        return View(nameof(CancellationConfirmation), model);
    }

    [HttpGet]
    [Route(PagePath.ConfirmRegistrationRefusal + "/{submissionId:guid}", Name = "ConfirmRegistrationRefusal")]
    public async Task<IActionResult> ConfirmRegistrationRefusal(Guid? submissionId)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedSubmissionId(submissionId, out var existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        SetBackLink(Url.RouteUrl("RejectRegistrationSubmission", new { submissionId }), false);

        var model = new ConfirmRegistrationRefusalViewModel
        {
            SubmissionId = existingModel.SubmissionId,
            RejectReason = existingModel.RejectReason
        };

        return View(nameof(ConfirmRegistrationRefusal), model);
    }

    [HttpPost]
    [Route(PagePath.ConfirmRegistrationRefusal + "/{submissionId:guid}", Name = "ConfirmRegistrationRefusal")]
    public async Task<IActionResult> ConfirmRegistrationRefusal(ConfirmRegistrationRefusalViewModel model)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedSubmissionId(model.SubmissionId, out var existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        if (!ModelState.IsValid)
        {
            SetBackLink(Url.RouteUrl("RejectRegistrationSubmission", new { model.SubmissionId }), false);
            return View(nameof(ConfirmRegistrationRefusal), model);
        }

        if (!model.IsRegistrationRefusalConfirmed.Value)
        {
            return RedirectToRoute("SubmissionDetails", new { existingModel.SubmissionId });
        }

        return await SubmitRegulatorRejectDecisionAsync(existingModel);

    }

    [HttpGet]
    [Route(PagePath.CancelDateRegistrationSubmission + "/{submissionId:guid}", Name = "CancelDateRegistrationSubmission")]
    public async Task<IActionResult> CancelDateRegistrationSubmission(Guid? submissionId)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedSubmissionId(submissionId, out var existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        SetBackLink($"{PagePath.CancelRegistrationSubmission}/{submissionId}");

        var model = new CancelDateRegistrationSubmissionViewModel
        {
            SubmissionId = submissionId.Value
        };

        ViewBag.BackToAllSubmissionsUrl = Url.Action("RegistrationSubmissions");

        return View(nameof(CancelDateRegistrationSubmission), model);
    }

    [HttpPost]
    [Route(PagePath.CancelDateRegistrationSubmission + "/{submissionId:guid}")]
    public async Task<IActionResult> CancelDateRegistrationSubmission(CancelDateRegistrationSubmissionViewModel model)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedSubmissionId(model.SubmissionId, out var existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        if (!ModelState.IsValid)
        {
            SetBackLink(Url.RouteUrl("CancelRegistrationSubmission", new { model.SubmissionId }), false);
            ViewBag.BackToAllSubmissionsUrl = Url.Action("RegistrationSubmissions");
            return View(nameof(CancelDateRegistrationSubmission), model);
        }

        try
        {
            var regulatorDecisionRequest = GetDecisionRequest(existingModel, Core.Enums.RegistrationSubmissionStatus.Cancelled);

            regulatorDecisionRequest.Comments = existingModel.CancellationReason;
            regulatorDecisionRequest.DecisionDate = model.CancellationDate;

            var status = await _facadeService.SubmitRegulatorRegistrationDecisionAsync(regulatorDecisionRequest);

            await UpdateOrganisationDetailsChangeHistoryAsync(existingModel, status, regulatorDecisionRequest);

            return status == Core.Models.EndpointResponseStatus.Success
                ? RedirectToRoute("CancellationConfirmation", new { submissionId = existingModel.SubmissionId })
                : RedirectToRoute("ServiceNotAvailable",
                new
                {
                    backLink = $"{PagePath.RegistrationSubmissionDetails}/{existingModel.SubmissionId}"
                });
        }
        catch (Exception ex)
        {
            _logControllerError.Invoke(
                logger,
                $"Exception received while cancelling submission" +
                $"{nameof(RegistrationSubmissionsController)}.{nameof(CancelDateRegistrationSubmission)}", ex);

            return RedirectToRoute(
                "ServiceNotAvailable",
                new
                {
                    backLink = $"{PagePath.RegistrationSubmissionDetails}/{existingModel.SubmissionId}"
                });
        }
    }

    [HttpGet]
    [Route(PagePath.RegistrationSubmissionsFileDownload)]
    public async Task<IActionResult> RegistrationSubmissionsFileDownload(string downloadType, [FromQuery] Guid submissionId)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        TempData["DownloadCompleted"] = false;
        _currentSession.RegulatorRegistrationSubmissionSession.FileDownloadRequestType = downloadType;
        await SaveSession(_currentSession);

        return RedirectToAction(nameof(SubmissionDetailsFileDownload), "RegistrationSubmissions", new { submissionId });
    }

    [HttpGet]
    [Route(PagePath.RegistrationSubmissionDetailsFileDownload)]
    public async Task<IActionResult> SubmissionDetailsFileDownload([FromQuery] Guid submissionId)
    {
        SetBackLink(Url.RouteUrl("SubmissionDetails", new { submissionId }), false);
        return View("RegistrationSubmissionFileDownload", submissionId);
    }

    [HttpGet]
    [Route("[controller]/" + nameof(FileDownloadInProgress))]
    public async Task<IActionResult> FileDownloadInProgress([FromQuery] Guid submissionId)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);
        var submission = await FetchFromSessionOrFacadeAsync(submissionId, _facadeService.GetRegistrationSubmissionDetails);
        if (submission is null)
        {
            _logControllerError.Invoke(logger, $"{submissionId} - submission not found - {nameof(RegistrationSubmissionsController)}.{nameof(RegistrationSubmissions)}", default);
            return RedirectToAction(nameof(PageNotFound));
        }

        var fileDownloadModel = CreateFileDownloadRequest(_currentSession, submission);
        if (fileDownloadModel == null)
        {
            return RedirectToAction(nameof(RegistrationSubmissionFileDownloadFailed), new { submissionId });
        }

        var response = await _facadeService.GetFileDownload(fileDownloadModel);

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            return RedirectToAction(nameof(RegistrationSubmissionFileDownloadSecurityWarning), new { submissionId });
        }

        response.EnsureSuccessStatusCode();
        var fileStream = await response.Content.ReadAsStreamAsync();
        var contentDisposition = response.Content.Headers.ContentDisposition;
        var fileName = contentDisposition?.FileNameStar ?? contentDisposition?.FileName ??
            submission.SubmissionDetails.Files[0].FileName ?? $"{submission.OrganisationReference}_details.csv";
        TempData["DownloadCompleted"] = true;

        return File(fileStream, "application/octet-stream", fileName);
    }

    [HttpGet]
    [Route(PagePath.RegistrationSubmissionFileDownloadFailed)]
    public async Task<IActionResult> RegistrationSubmissionFileDownloadFailed([FromQuery] Guid submissionId)
    {
        var model = new OrganisationDetailsFileDownloadViewModel(true, false, submissionId);
        return View("RegistrationSubmissionFileDownloadFailed", model);
    }

    [HttpGet]
    [Route(PagePath.RegistrationSubmissionFileDownloadSecurityWarning)]
    public async Task<IActionResult> RegistrationSubmissionFileDownloadSecurityWarning([FromQuery] Guid submissionId)
    {
        var model = new OrganisationDetailsFileDownloadViewModel(true, true, submissionId);
        return View("RegistrationSubmissionFileDownloadFailed", model);
    }

    [HttpGet]
    [Route(PagePath.PageNotFoundPath)]
    public async Task<IActionResult> PageNotFound()
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);
        if (_currentSession is not null)
        {
            _currentSession.RegulatorRegistrationSubmissionSession.CurrentPageNumber = 1;
            await SaveSessionAndJourney(_currentSession.RegulatorRegistrationSubmissionSession, PagePath.RegistrationSubmissionsRoute, PagePath.PageNotFound);
        }
        return RedirectToAction(PagePath.Error, "Error", new { statusCode = 404, backLink = PagePath.RegistrationSubmissionsRoute });
    }

    public static void InitialiseOrContinuePaging(RegulatorRegistrationSubmissionSession session,
                                            int? pageNumber) => session.CurrentPageNumber = pageNumber ?? session.CurrentPageNumber ?? 1;

    private RegistrationSubmissionsViewModel InitialiseOrCreateViewModel(
        RegulatorRegistrationSubmissionSession session,
        int nationId)
    {
        RegistrationSubmissionsFilterViewModel existingSessionFilters = session.LatestFilterChoices != null
            ? (RegistrationSubmissionsFilterViewModel)session.LatestFilterChoices
            : new RegistrationSubmissionsFilterViewModel
            {
                PageNumber = 1,
                PageSize = 20,
                NationId = nationId
            };

        int currentPageNumber = session.CurrentPageNumber ?? 1;
        existingSessionFilters.PageNumber = currentPageNumber;
        existingSessionFilters.SubmissionYears = _submissionFiltersConfig.ParseSubmissionYears()
            .Select(y => y.ToString())
            .ToArray();
        if (session.LatestFilterChoices != null)
        {
            session.LatestFilterChoices.PageNumber = currentPageNumber;
        }

        return new RegistrationSubmissionsViewModel
        {
            NationId = nationId,
            ListViewModel = new RegistrationSubmissionsListViewModel
            {
                NationId = nationId,
                RegistrationsFilterViewModel = existingSessionFilters,
                PaginationNavigationModel = new ViewModels.Shared.PaginationNavigationModel
                {
                    CurrentPage = session.CurrentPageNumber ?? 1
                }
            },
            PowerBiLogin = _externalUrlsOptions.PowerBiLogin,
            AgencyName = GetRegulatorAgencyName(nationId)
        };
    }

    private bool GetOrRejectProvidedSubmissionId(Guid? submissionId, out RegistrationSubmissionDetailsViewModel viewModel)
    {
        viewModel = null;

        if (!submissionId.HasValue)
        {
            return false;
        }

        var regulatorRegSubSession = _currentSession.RegulatorRegistrationSubmissionSession;
        var orgDetailsChangeHistory = regulatorRegSubSession.OrganisationDetailsChangeHistory.TryGetValue(submissionId.Value, out var value1)
                                        ? value1 : null;
        var selectedRegistrations = regulatorRegSubSession.SelectedRegistrations.TryGetValue(submissionId.Value, out var value2)
                                        ? value2 : null;
        var sessionModelWhichMustMatchSession = orgDetailsChangeHistory ?? selectedRegistrations;

        if (sessionModelWhichMustMatchSession is null)
        {
            return false;
        }

        viewModel = (RegistrationSubmissionDetailsViewModel)sessionModelWhichMustMatchSession;
        return true;
    }

    private async Task<RegistrationSubmissionOrganisationDetails> FetchFromSessionOrFacadeAsync(Guid submissionId, Func<Guid, Task<RegistrationSubmissionOrganisationDetails>> facadeMethod)
    {
        if (_currentSession.RegulatorRegistrationSubmissionSession.SelectedRegistrations.TryGetValue(submissionId, out var selectedRegistration))
        {
            return selectedRegistration;
        }

        var submission = await facadeMethod(submissionId);
        if (submission is not null)
        {
            _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);
            _currentSession.RegulatorRegistrationSubmissionSession.SelectedRegistrations[submission.SubmissionId] = submission;
            SaveSession(_currentSession);
        }

        return submission;
    }

    private static void ClearFilters(RegulatorRegistrationSubmissionSession session,
                              RegistrationSubmissionsFilterViewModel filters,
                              bool performClearance)
    {
        if (!performClearance)
        {
            session.ClearFilters = false;
            if (null != filters)
            {
                filters.ClearFilters = false;
            }
            return;
        }

        session.ClearFilters = true;
        if (null != filters)
        {
            filters.ClearFilters = true;
        }
    }

    private static void UpdateRegistrationSubmissionFiltersInSession(
                        RegulatorRegistrationSubmissionSession session,
                        RegistrationSubmissionsFilterViewModel filters,
                        bool performUpdate)
    {
        if (!performUpdate)
        {
            return;
        }

        session.LatestFilterChoices = filters;
    }

    private static void GeneratePowerBILink(RegistrationSubmissionDetailsViewModel model) => model.PowerBiLogin = "https://app.powerbi.com/";

    private RedirectToActionResult? ReturnIfAppropriate(RegistrationSubmissionsFilterViewModel? filters, string? filterType) =>
            (filters, filterType) switch
            {
                (null, null) => RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions"),
                (null, FilterActions.SubmitFilters) => RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions"),
                (_, not FilterActions.ClearFilters and not FilterActions.SubmitFilters) => RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions"),
                _ => null
            };

    private async Task SaveSessionAndJourney(RegulatorRegistrationSubmissionSession session, string currentPagePath, string? nextPagePath)
    {
        ClearRestOfJourney(session, currentPagePath);

        session.Journey.AddIfNotExists(nextPagePath);

        await SaveSession(_currentSession);
    }

    private static void ClearRestOfJourney(RegulatorRegistrationSubmissionSession session, string currentPagePath)
    {
        int index = session.Journey.IndexOf(currentPagePath);
        session.Journey = session.Journey.Take(index + 1).ToList();
    }

    private async Task SaveSession(JourneySession session) =>
        await _sessionManager.SaveSessionAsync(HttpContext.Session, session);


    private void SetBacklinkToHome()
    {
        string pathBase = _pathBase.TrimStart('/').TrimEnd('/');
        ViewBag.CustomBackLinkToDisplay = $"/{pathBase}/{PagePath.Home}";
    }

    private void SetBackLink(string path, bool hasPathBase = true)
    {
        if (hasPathBase)
        {
            string pathBase = _pathBase.TrimStart('/').TrimEnd('/');
            ViewBag.BackLinkToDisplay = $"/{pathBase}/{path}";
        }
        else
        {
            ViewBag.BackLinkToDisplay = path;
        }
    }

    private async Task<IActionResult> ProcessOfflinePaymentAsync(RegistrationSubmissionDetailsViewModel existingModel, string offlinePayment)
    {
        string regulator = ((CountryName)existingModel.NationId).GetDescription();
        var response = await _paymentFacadeService.SubmitOfflinePaymentAsync(new OfflinePaymentRequest
        {
            Amount = (int)(decimal.Parse(offlinePayment, CultureInfo.InvariantCulture) * 100),
            Description = "Registration fee",
            Reference = existingModel.ReferenceNumber,
            Regulator = regulator,
            UserId = (Guid)_currentSession.UserData.Id
        });

        if (response == EndpointResponseStatus.Fail)
        {
            return RedirectToRoute("ServiceNotAvailable", new { backLink = $"{PagePath.RegistrationSubmissionDetails}/{existingModel.SubmissionId}" });
        }

        await _facadeService.SubmitRegistrationFeePaymentAsync(new FeePaymentRequest
        {
            PaidAmount = offlinePayment,
            PaymentMethod = "Offline",
            PaymentStatus = "Paid",
            SubmissionId = existingModel.SubmissionId,
            UserId = (Guid)_currentSession.UserData.Id
        });

        return Redirect(Url.RouteUrl("SubmissionDetails", new { existingModel.SubmissionId }));
    }

    private static string GetRegulatorAgencyName(int nationId) => nationId switch
    {
        1 => "Environment Agency (EA)",
        2 => "Northern Ireland Environment Agency (NIEA)",
        3 => "Scottish Environment Protection Agency (SEPA)",
        4 => "Natural Resources Wales (NRW)",
        _ => "",
    };

    private static string GetRegulatorAgencyEmail(int nationId) => nationId switch
    {
        1 => "packagingproducers@environment-agency.gov.uk",
        2 => "packaging@daera-ni.gov.uk",
        3 => "producer.responsibility@sepa.org.uk",
        4 => "deunyddpacio@cyfoethnaturiolcymru.gov.uk; packaging@naturalresourceswales.gov.uk",
        _ => "",
    };

    private static string GetCountryCodeInitial(int nationId)
    {
        string code = nationId switch
        {
            1 => "Eng",
            2 => "NI",
            3 => "Sco",
            4 => "Wal",
            _ => "Eng",
        };
        return code;
    }

    private async Task UpdateOrganisationDetailsChangeHistoryAsync(RegistrationSubmissionDetailsViewModel existingModel, EndpointResponseStatus status, RegulatorDecisionRequest regulatorDecisionRequest)
    {
        if (status == EndpointResponseStatus.Success)
        {
            existingModel.RegulatorComments = regulatorDecisionRequest.Comments;
            UpdateRegistrationSubmissionDetailsViewModel(existingModel, regulatorDecisionRequest);

            if (_currentSession!.RegulatorRegistrationSubmissionSession.OrganisationDetailsChangeHistory.TryGetValue(existingModel.SubmissionId, out _))
            {
                _currentSession.RegulatorRegistrationSubmissionSession.OrganisationDetailsChangeHistory[existingModel.SubmissionId] = existingModel;
            }
            else
            {
                _currentSession!.RegulatorRegistrationSubmissionSession.OrganisationDetailsChangeHistory.Add(existingModel.SubmissionId, existingModel);
            }
            await SaveSession(_currentSession);
        }
    }

    private static void UpdateRegistrationSubmissionDetailsViewModel(RegistrationSubmissionDetailsViewModel existingModel, RegulatorDecisionRequest regulatorDecisionRequest)
    {
        if (existingModel.IsResubmission && "Granted Refused".Contains(regulatorDecisionRequest.Status))
        {
            existingModel.ResubmissionStatus = regulatorDecisionRequest.Status switch
            {
                "Granted" => RegistrationSubmissionStatus.Accepted,
                "Refused" => RegistrationSubmissionStatus.Rejected,
                _ => existingModel.ResubmissionStatus
            };
            existingModel.SubmissionDetails.ResubmissionStatus = existingModel.ResubmissionStatus;
            existingModel.SubmissionDetails.ResubmissionDecisionDate = DateTime.UtcNow;
        }
        else
        {
            existingModel.Status = Enum.Parse<RegistrationSubmissionStatus>(regulatorDecisionRequest.Status, true);
            existingModel.SubmissionDetails.Status = existingModel.Status;
            existingModel.SubmissionDetails.LatestDecisionDate = DateTime.UtcNow;
            existingModel.SubmissionDetails.StatusPendingDate = regulatorDecisionRequest.Status == "Cancelled" ? regulatorDecisionRequest.DecisionDate : null;
        }
    }

    private static RegulatorDecisionRequest GetDecisionRequest(
        RegistrationSubmissionDetailsViewModel existingModel,
        RegistrationSubmissionStatus status)
    {
        var request = new RegulatorDecisionRequest
        {
            ApplicationReferenceNumber = existingModel.ReferenceNumber,
            OrganisationId = existingModel.OrganisationId,
            SubmissionId = existingModel.SubmissionId,
            // For generating reference and send email
            TwoDigitYear = (existingModel.RegistrationYear % 100).ToString(CultureInfo.InvariantCulture),
            OrganisationAccountManagementId = existingModel.OrganisationReference,
            // For sending emails
            OrganisationName = existingModel.OrganisationName,
            OrganisationEmail = existingModel.SubmissionDetails.Email,
            OrganisationReference = existingModel.OrganisationReference,
            AgencyName = GetRegulatorAgencyName(existingModel.NationId),
            AgencyEmail = GetRegulatorAgencyEmail(existingModel.NationId),
            IsWelsh = existingModel.NationId == 4,
            Status = status.ToString(),
            IsResubmission = existingModel.IsResubmission,
            FileId = status switch
            {
                RegistrationSubmissionStatus.Cancelled => null,
                _ => existingModel.IsResubmission ? existingModel.ResubmissionFileId : null
            }
        };

        if (request.IsResubmission)
        {
            request.ExistingRegRefNumber = existingModel.RegistrationReferenceNumber;
            return request;
        }

        // For generating reference
        request.CountryName = GetCountryCodeInitial(existingModel.NationId);
        request.RegistrationSubmissionType = existingModel.OrganisationType.GetRegistrationSubmissionType();
        return request;
    }

    private static FileDownloadRequest CreateFileDownloadRequest(JourneySession session, RegistrationSubmissionOrganisationDetails registration)
    {
        var fileDownloadModel = new FileDownloadRequest
        {
            SubmissionId = registration.SubmissionId,
            SubmissionType = SubmissionType.Registration
        };

        switch (session.RegulatorRegistrationSubmissionSession.FileDownloadRequestType)
        {
            case FileDownloadTypes.OrganisationDetails:
                var orgFile = registration.SubmissionDetails.Files.Find(static x => x.Type == RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileType.company);
                if (null != orgFile)
                {
                    fileDownloadModel.FileId = orgFile.FileId;
                    fileDownloadModel.BlobName = orgFile.BlobName;
                    fileDownloadModel.FileName = orgFile.FileName;
                }
                break;
            case FileDownloadTypes.BrandDetails:
                orgFile = registration.SubmissionDetails.Files.Find(static x => x.Type == RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileType.brands);
                if (null != orgFile)
                {
                    fileDownloadModel.FileId = orgFile.FileId;
                    fileDownloadModel.BlobName = orgFile.BlobName;
                    fileDownloadModel.FileName = orgFile.FileName;
                }
                break;
            case FileDownloadTypes.PartnershipDetails:
                orgFile = registration.SubmissionDetails.Files.Find(static x => x.Type == RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileType.partnership);
                if (null != orgFile)
                {
                    fileDownloadModel.FileId = orgFile.FileId;
                    fileDownloadModel.BlobName = orgFile.BlobName;
                    fileDownloadModel.FileName = orgFile.FileName;
                }
                break;
            default:
                return null;
        }

        if (fileDownloadModel.FileId == null || fileDownloadModel.BlobName == null || fileDownloadModel.FileName == null)
        {
            return null;
        }

        return fileDownloadModel;
    }

    private async Task<IActionResult> SubmitRegulatorRejectDecisionAsync(RegistrationSubmissionDetailsViewModel registrationSubmissionDetailsViewModel)
    {
        try
        {
            var regulatorDecisionRequest = GetDecisionRequest(registrationSubmissionDetailsViewModel, Core.Enums.RegistrationSubmissionStatus.Refused);

            regulatorDecisionRequest.Comments = registrationSubmissionDetailsViewModel.RejectReason;

            var status = await _facadeService.SubmitRegulatorRegistrationDecisionAsync(regulatorDecisionRequest);

            await UpdateOrganisationDetailsChangeHistoryAsync(registrationSubmissionDetailsViewModel, status, regulatorDecisionRequest);

            return status == Core.Models.EndpointResponseStatus.Success
                ? RedirectToAction(PagePath.RegistrationSubmissionsAction)
                : RedirectToRoute("ServiceNotAvailable",
                new
                {
                    backLink = $"{PagePath.RegistrationSubmissionDetails}/{registrationSubmissionDetailsViewModel.SubmissionId}"
                });
        }
        catch (Exception ex)
        {
            _logControllerError.Invoke(
                logger,
                $"Exception received while refusing submission" +
                $"{nameof(RegistrationSubmissionsController)}.{nameof(ConfirmRegistrationRefusal)}", ex);

            return RedirectToRoute(
                "ServiceNotAvailable",
                new
                {
                    backLink = $"{PagePath.RegistrationSubmissionDetails}/{registrationSubmissionDetailsViewModel.SubmissionId}"
                });
        }
    }
}