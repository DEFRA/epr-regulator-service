using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text.Encodings.Web;

using EPR.Common.Authorization.Constants;
using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.Registrations;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement.Mvc;

using ServiceRole = EPR.RegulatorService.Frontend.Core.Enums.ServiceRole;

namespace EPR.RegulatorService.Frontend.Web.Controllers.RegistrationSubmissions;

[FeatureGate(FeatureFlags.ManageRegistrationSubmissions)]
[Authorize(Policy = PolicyConstants.RegulatorBasicPolicy)]
public partial class RegistrationSubmissionsController(
                IFacadeService facade,
                IPaymentFacadeService paymentFacade,
                ISessionManager<JourneySession> sessionManager,
                ILogger<RegistrationSubmissionsController> logger,
                IConfiguration configuration,
                IOptions<ExternalUrlsOptions> externalUrlsOptions
             ) : Controller
{
    private readonly string _pathBase = configuration.GetValue<string>(ConfigKeys.PathBase);
    private readonly ExternalUrlsOptions _externalUrlsOptions = externalUrlsOptions.Value;
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
        try
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
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            _logControllerError.Invoke(logger, $"Exception received processing GET to {nameof(RegistrationSubmissionsController)}.{nameof(RegistrationSubmissions)}", ex);
            return RedirectToAction(PagePath.Error, "Error");
        }
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
            Debug.WriteLine(ex);
            _logControllerError.Invoke(logger, $"Exception received processing POST to {nameof(RegistrationSubmissionsController)}.{nameof(RegistrationSubmissions)}", ex);
            return RedirectToAction(PagePath.Error, "Error");
        }

        return RedirectToAction(PagePath.RegistrationSubmissionsAction);
    }

    [HttpGet]
    [Route(PagePath.RegistrationSubmissionDetails + "/{submissionId}", Name = "SubmissionDetails")]
    public async Task<IActionResult> RegistrationSubmissionDetails(string submissionId)
    {
        try
        {
            var subId = GetDecodedSubmissionId(submissionId);
            _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);
            RegistrationSubmissionDetailsViewModel model = await FetchFromSessionOrFacadeAsync(subId, _facadeService.GetRegistrationSubmissionDetails);

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
            Debug.WriteLine(ex);
            _logControllerError.Invoke(logger, $"Exception received processing GET to {nameof(RegistrationSubmissionsController)}.{nameof(RegistrationSubmissions)}", ex);
            return RedirectToAction(PagePath.Error, "Error");
        }
    }

    [HttpPost]
    [Route(PagePath.RegistrationSubmissionDetails + "/{submissionId}", Name = "SubmitPaymentInfo")]
    public async Task<IActionResult> SubmitOfflinePayment([FromForm] PaymentDetailsViewModel paymentDetailsViewModel, [FromRoute] string submissionId)
    {
        var subId = GetDecodedSubmissionId(submissionId);
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedSubmissionId(subId, out var existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        if (!ModelState.IsValid)
        {
            return View(nameof(RegistrationSubmissionDetails), existingModel);
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
    [Route(PagePath.GrantRegistrationSubmission + "/{submissionId}", Name = "GrantRegistrationSubmission")]
    public async Task<IActionResult> GrantRegistrationSubmission(string? submissionId)
    {
        var subId = GetDecodedSubmissionId(submissionId);
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedSubmissionId(subId, out var existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        SetBackLink($"{PagePath.RegistrationSubmissionDetails}/{submissionId}");

        var model = new GrantRegistrationSubmissionViewModel
        {
            SubmissionId = submissionId,
            IsResubmission = existingModel.IsResubmission
        };

        ViewBag.BackToAllSubmissionsUrl = Url.Action("RegistrationSubmissions");

        return View(nameof(GrantRegistrationSubmission), model);
    }

    [HttpPost]
    [Route(PagePath.GrantRegistrationSubmission + "/{submissionId}", Name = "GrantRegistrationSubmission")]
    public async Task<IActionResult> GrantRegistrationSubmission(GrantRegistrationSubmissionViewModel model)
    {
        var subId = GetDecodedSubmissionId(model.SubmissionId);
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedSubmissionId(subId, out var existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        if (!ModelState.IsValid)
        {
            SetBackLink($"{PagePath.RegistrationSubmissionDetails}/{model.SubmissionId}");
            ViewBag.BackToAllSubmissionsUrl = Url.Action("RegistrationSubmissions");
            return View(nameof(GrantRegistrationSubmission), model);
        }

        if (!model.IsGrantRegistrationConfirmed.Value)
        {
            return RedirectToRoute("SubmissionDetails", new { model.SubmissionId });
        }

        try
        {
            var regulatorDecisionRequest = GetDecisionRequest(existingModel, RegistrationSubmissionStatus.Granted);
            var status = await _facadeService.SubmitRegulatorRegistrationDecisionAsync(regulatorDecisionRequest);

            _currentSession.RegulatorRegistrationSubmissionSession.SelectedRegistrations[existingModel.SubmissionId] = null;  // this will force a reload of the item in SubmissionDetails

            SaveSession(_currentSession);

            return status == EndpointResponseStatus.Success
                  ? RedirectToRoute("SubmissionDetails", new { model.SubmissionId })
                  : RedirectToRoute("ServiceNotAvailable", new { backLink = $"{PagePath.RegistrationSubmissionDetails}/{model.SubmissionId}" });
        }
        catch (Exception ex)
        {
            _logControllerError.Invoke(logger, $"Exception received while granting submission {nameof(RegistrationSubmissionsController)}.{nameof(GrantRegistrationSubmission)}", ex);
            return RedirectToRoute("ServiceNotAvailable", new { backLink = $"{PagePath.RegistrationSubmissionDetails}/{model.SubmissionId}" });
        }
    }

    [HttpGet]
    [Route(PagePath.QueryRegistrationSubmission + "/{submissionId}", Name = "QueryRegistrationSubmission")]
    public async Task<IActionResult> QueryRegistrationSubmission(string? submissionId)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        ////if (!GetOrRejectProvidedSubmissionId(subId, out RegistrationSubmissionDetailsViewModel existingModel))
        ////{
        ////    return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        ////}

        SetBackLink(Url.RouteUrl("SubmissionDetails", new { submissionId }), false);

        var model = new QueryRegistrationSubmissionViewModel
        {
            SubmissionId = submissionId
        };

        ViewBag.BackToAllSubmissionsUrl = Url.Action("RegistrationSubmissions");

        return View(nameof(QueryRegistrationSubmission), model);
    }

    [HttpPost]
    [Route(PagePath.QueryRegistrationSubmission + "/{submissionId}", Name = "QueryRegistrationSubmission")]
    public async Task<IActionResult> QueryRegistrationSubmission(QueryRegistrationSubmissionViewModel model)
    {
        var subId = GetDecodedSubmissionId(model.SubmissionId);
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedSubmissionId(subId, out var existingModel))
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
            var regulatorDecisionRequest = GetDecisionRequest(existingModel, RegistrationSubmissionStatus.Queried);
            regulatorDecisionRequest.Comments = model.Query;

            var status = await _facadeService.SubmitRegulatorRegistrationDecisionAsync(regulatorDecisionRequest);

            await UpdateOrganisationDetailsChangeHistoryAsync(existingModel, status, regulatorDecisionRequest);

            return status == EndpointResponseStatus.Success
                ? RedirectToAction(PagePath.RegistrationSubmissionsAction)
                : RedirectToRoute("ServiceNotAvailable",
                new
                {
                    backLink = $"{PagePath.RegistrationSubmissionDetails}/{model.SubmissionId}"
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
                    backLink = $"{PagePath.RegistrationSubmissionDetails}/{model.SubmissionId}"
                });
        }
    }

    [HttpGet]
    [Route(PagePath.RejectRegistrationSubmission + "/{submissionId}", Name = "RejectRegistrationSubmission")]
    public async Task<IActionResult> RejectRegistrationSubmission(string? submissionId)
    {
        var subId = GetDecodedSubmissionId(submissionId);
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedSubmissionId(subId, out RegistrationSubmissionDetailsViewModel existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }
        SetBackLink($"{PagePath.RegistrationSubmissionDetails}/{submissionId}");

        var model = new RejectRegistrationSubmissionViewModel
        {
            SubmissionId = submissionId,
            IsResubmission = existingModel.IsResubmission
        };

        ViewBag.BackToAllSubmissionsUrl = Url.Action("RegistrationSubmissions");

        return View(nameof(RejectRegistrationSubmission), model);
    }

    [HttpPost]
    [Route(PagePath.RejectRegistrationSubmission + "/{submissionId}", Name = "RejectRegistrationSubmission")]
    public async Task<IActionResult> RejectRegistrationSubmission(RejectRegistrationSubmissionViewModel model)
    {
        var subId = GetDecodedSubmissionId(model.SubmissionId);
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedSubmissionId(subId, out var existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        if (!ModelState.IsValid)
        {
            SetBackLink(Url.RouteUrl("SubmissionDetails", new { model.SubmissionId }), false);
            ViewBag.BackToAllSubmissionsUrl = Url.Action("RegistrationSubmissions");
            return View(nameof(RejectRegistrationSubmission), model);
        }

        _currentSession.RegulatorRegistrationSubmissionSession.SelectedRegistrations[subId].RejectReason = model.RejectReason;
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
            ? await SubmitRegulatorRejectDecisionAsync(existingModel, model.SubmissionId)
            : RedirectToRoute("ConfirmRegistrationRefusal", new { submissionId = model.SubmissionId });
    }

    [HttpGet]
    [Route(PagePath.CancelRegistrationSubmission + "/{submissionId}", Name = "CancelRegistrationSubmission")]
    public async Task<IActionResult> CancelRegistrationSubmission(string? submissionId)
    {
        var subId = GetDecodedSubmissionId(submissionId);
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedSubmissionId(subId, out var existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        SetBackLink($"{PagePath.RegistrationSubmissionDetails}/{submissionId}");

        var model = new CancelRegistrationSubmissionViewModel
        {
            SubmissionId = submissionId
        };

        ViewBag.BackToAllSubmissionsUrl = Url.Action("RegistrationSubmissions");

        return View(nameof(CancelRegistrationSubmission), model);
    }

    [HttpPost]
    [Route(PagePath.CancelRegistrationSubmission + "/{submissionId}", Name = "CancelRegistrationSubmission")]
    public async Task<IActionResult> CancelRegistrationSubmission(CancelRegistrationSubmissionViewModel model)
    {
        var subId = GetDecodedSubmissionId(model.SubmissionId);
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedSubmissionId(subId, out var existingModel))
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

        return RedirectToRoute("CancelDateRegistrationSubmission", new { submissionId = model.SubmissionId });
    }

    [HttpGet]
    [Route(PagePath.ConfirmOfflinePaymentSubmission + "/{submissionId}", Name = "ConfirmOfflinePaymentSubmission")]
    public async Task<IActionResult> ConfirmOfflinePaymentSubmission(string? submissionId)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        string offlinePayment = TempData.Peek("OfflinePaymentAmount")?.ToString();
        if (string.IsNullOrWhiteSpace(offlinePayment))
        {
            return RedirectToAction(PagePath.RegistrationSubmissionDetails, "RegistrationSubmissions", new { submissionId});
        }

        SetBackLink(Url.RouteUrl("SubmissionDetails", new { submissionId }), false);

        var model = new ConfirmOfflinePaymentSubmissionViewModel
        {
            SubmissionId = submissionId,
            OfflinePaymentAmount = offlinePayment
        };

        return View(nameof(ConfirmOfflinePaymentSubmission), model);
    }

    [HttpPost]
    [Route(PagePath.ConfirmOfflinePaymentSubmission + "/{submissionId}", Name = "ConfirmOfflinePaymentSubmission")]
    public async Task<IActionResult> ConfirmOfflinePaymentSubmission(ConfirmOfflinePaymentSubmissionViewModel model)
    {
        var subId = GetDecodedSubmissionId(model.SubmissionId);
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedSubmissionId(subId, out var existingModel))
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
            return RedirectToRoute("SubmissionDetails", new { model.SubmissionId });
        }

        TempData.Remove("OfflinePaymentAmount");
        return string.IsNullOrWhiteSpace(model.OfflinePaymentAmount)
            ? RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions")
            : await ProcessOfflinePaymentAsync(existingModel, model.OfflinePaymentAmount);
    }

    [HttpGet]
    [Route(PagePath.CancellationConfirmation + "/{submissionId}", Name = "CancellationConfirmation")]
    public async Task<IActionResult> CancellationConfirmation(string? submissionId)
    {
        var subId = GetDecodedSubmissionId(submissionId);
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedSubmissionId(subId, out var existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        if (string.IsNullOrWhiteSpace(existingModel.OrganisationName))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        var model = new CancellationConfirmationViewModel
        {
            SubmissionId = submissionId,
            OrganisationName = existingModel.OrganisationName
        };

        ViewBag.BackToAllSubmissionsUrl = Url.Action("RegistrationSubmissions");

        return View(nameof(CancellationConfirmation), model);
    }

    [HttpGet]
    [Route(PagePath.ConfirmRegistrationRefusal + "/{submissionId}", Name = "ConfirmRegistrationRefusal")]
    public async Task<IActionResult> ConfirmRegistrationRefusal(string? submissionId)
    {
        var subId = GetDecodedSubmissionId(submissionId);
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedSubmissionId(subId, out var existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        SetBackLink(Url.RouteUrl("RejectRegistrationSubmission", new { submissionId }), false);

        var model = new ConfirmRegistrationRefusalViewModel
        {
            SubmissionId = submissionId,
            RejectReason = existingModel.RejectReason
        };

        return View(nameof(ConfirmRegistrationRefusal), model);
    }

    [HttpPost]
    [Route(PagePath.ConfirmRegistrationRefusal + "/{submissionId}", Name = "ConfirmRegistrationRefusal")]
    public async Task<IActionResult> ConfirmRegistrationRefusal(ConfirmRegistrationRefusalViewModel model)
    {
        var subId = GetDecodedSubmissionId(model.SubmissionId);
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedSubmissionId(subId, out var existingModel))
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
            return RedirectToRoute("SubmissionDetails", new { model.SubmissionId });
        }

        return await SubmitRegulatorRejectDecisionAsync(existingModel, model.SubmissionId);

    }

    [HttpGet]
    [Route(PagePath.CancelDateRegistrationSubmission + "/{submissionId}", Name = "CancelDateRegistrationSubmission")]
    public async Task<IActionResult> CancelDateRegistrationSubmission(string? submissionId)
    {
        var subId = GetDecodedSubmissionId(submissionId);
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedSubmissionId(subId, out var existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        SetBackLink($"{PagePath.CancelRegistrationSubmission}/{submissionId}");

        var model = new CancelDateRegistrationSubmissionViewModel
        {
            SubmissionId = submissionId
        };

        ViewBag.BackToAllSubmissionsUrl = Url.Action("RegistrationSubmissions");

        return View(nameof(CancelDateRegistrationSubmission), model);
    }

    [HttpPost]
    [Route(PagePath.CancelDateRegistrationSubmission + "/{submissionId}")]
    public async Task<IActionResult> CancelDateRegistrationSubmission(CancelDateRegistrationSubmissionViewModel model)
    {
        var subId = GetDecodedSubmissionId(model.SubmissionId);
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedSubmissionId(subId, out var existingModel))
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
    public async Task<IActionResult> RegistrationSubmissionsFileDownload(string downloadType, [FromQuery] string submissionId)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        TempData["DownloadCompleted"] = false;
        _currentSession.RegulatorRegistrationSubmissionSession.FileDownloadRequestType = downloadType;
        await SaveSession(_currentSession);

        return RedirectToAction(nameof(SubmissionDetailsFileDownload), "RegistrationSubmissions", new { submissionId });
    }

    [HttpGet]
    [Route(PagePath.RegistrationSubmissionDetailsFileDownload)]
    public async Task<IActionResult> SubmissionDetailsFileDownload([FromQuery] string submissionId)
    {
        SetBackLink(Url.RouteUrl("SubmissionDetails", new { submissionId }), false);
        return View("RegistrationSubmissionFileDownload", model: submissionId);
    }

    [HttpGet]
    public async Task<IActionResult> FileDownloadInProgress([FromQuery]string submissionId)
    {
        var subId = GetDecodedSubmissionId(submissionId);
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        var registration = _currentSession.RegulatorRegistrationSubmissionSession.SelectedRegistrations[subId];
        var fileDownloadModel = CreateFileDownloadRequest(_currentSession, registration);

        if (fileDownloadModel == null)
        {
            return RedirectToAction(nameof(RegistrationSubmissionFileDownloadFailed));
        }

        var response = await _facadeService.GetFileDownload(fileDownloadModel);

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            return RedirectToAction(nameof(RegistrationSubmissionFileDownloadSecurityWarning), new { submissionId });
        }
        else if (response.IsSuccessStatusCode)
        {
            var fileStream = await response.Content.ReadAsStreamAsync();
            var contentDisposition = response.Content.Headers.ContentDisposition;
            var fileName = contentDisposition?.FileNameStar ?? contentDisposition?.FileName ?? registration.SubmissionDetails.Files[0].FileName ?? $"{registration.OrganisationReference}_details.csv";
            TempData["DownloadCompleted"] = true;

            return File(fileStream, "application/octet-stream", fileName);
        }
        else
        {
            return RedirectToAction(nameof(RegistrationSubmissionFileDownloadFailed), new { submissionId } );
        }
    }    

    [HttpGet]
    [Route(PagePath.RegistrationSubmissionFileDownloadFailed)]
    public async Task<IActionResult> RegistrationSubmissionFileDownloadFailed([FromQuery] string submissionId)
    {
        var model = new OrganisationDetailsFileDownloadViewModel(true, false, submissionId);
        return View("RegistrationSubmissionFileDownloadFailed", model);
    }

    [HttpGet]
    [Route(PagePath.RegistrationSubmissionFileDownloadSecurityWarning)]
    public async Task<IActionResult> RegistrationSubmissionFileDownloadSecurityWarning([FromQuery] string submissionId)
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

    private static Guid GetDecodedSubmissionId(string submissionId) => new(Convert.FromBase64String(Uri.UnescapeDataString(submissionId)));
}