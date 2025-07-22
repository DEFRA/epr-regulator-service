using System.Globalization;
using System.Net;

using EPR.Common.Authorization.Constants;
using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
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
using Mappers;

using ViewModels.Shared;

[FeatureGate(FeatureFlags.ManageRegistrationSubmissions)]
[Authorize(Policy = PolicyConstants.RegulatorBasicPolicy)]
public partial class RegistrationSubmissionsController(
    IFacadeService facade,
    IPaymentFacadeService paymentFacade,
    ISessionManager<JourneySession> sessionManager,
    ILogger<RegistrationSubmissionsController> logger,
    IConfiguration configuration,
    IOptions<ExternalUrlsOptions> externalUrlsOptions,
    IOptions<RegistrationSubmissionsConfig> registrationSubmissionsConfig) : Controller
{
    private readonly string _pathBase = configuration.GetValue<string>(ConfigKeys.PathBase);
    private readonly ExternalUrlsOptions _externalUrlsOptions = externalUrlsOptions.Value;
    private readonly RegistrationSubmissionsConfig _registrationSubmissionsConfig = registrationSubmissionsConfig.Value;
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

            var pagedOrganisationRegistrations = await _facadeService.GetRegistrationSubmissions(viewModel.ListViewModel.RegistrationsFilterModel);

            if (pagedOrganisationRegistrations?.items?.Count > 0)
            {

                viewModel.ListViewModel.PagedRegistrationSubmissions =
                    pagedOrganisationRegistrations.items.Select(RegistrationSubmissionDetailsStaticMapper.MapFromOrganisationDetails);

                viewModel.ListViewModel.PaginationNavigationModel = new PaginationNavigationModel
                {
                    CurrentPage = pagedOrganisationRegistrations.currentPage,
                    PageCount = pagedOrganisationRegistrations.TotalPages,
                    ControllerName = "RegistrationSubmissions",
                    ActionName = nameof(RegistrationSubmissions)
                };

                if ((viewModel.ListViewModel.PaginationNavigationModel.CurrentPage > pagedOrganisationRegistrations.TotalPages &&
                     viewModel.ListViewModel.PaginationNavigationModel.CurrentPage > 1) || viewModel.ListViewModel.PaginationNavigationModel.CurrentPage < 1)
                {
                    viewModel.ListViewModel.PaginationNavigationModel.CurrentPage = 1;
                }

                // cache selected organisation types for the session
                _currentSession.RegulatorRegistrationSubmissionSession.SelectedOrganisationTypes = viewModel.ListViewModel.PagedRegistrationSubmissions
                    .ToDictionary(x => x.SubmissionId, x => x.OrganisationType);
            }

            await SaveSessionAndJourney(_currentSession.RegulatorRegistrationSubmissionSession, PagePath.RegistrationSubmissionsRoute, PagePath.RegistrationSubmissionsRoute);

            return View(viewModel);
        }
        catch (Exception ex)
        {
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

            if (!GetOrRejectProvidedSubmissionId(submissionId.Value, out var model))
            {
                if (!_currentSession.RegulatorRegistrationSubmissionSession.SelectedOrganisationTypes.TryGetValue(
                        submissionId.Value, out RegistrationSubmissionOrganisationType organisationType))
                {
                    return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
                }

                var registrationSubmissionOrganisationDetails = await FetchFromSessionOrFacadeAsync(submissionId.Value, organisationType, _facadeService.GetRegistrationSubmissionDetails);
                model = RegistrationSubmissionDetailsStaticMapper.MapFromOrganisationDetails(registrationSubmissionOrganisationDetails);
            }

            if (model is null)
            {
                return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
            }

            GeneratePowerBILink(model);
            SetBackLink(PagePath.RegistrationSubmissionsRoute);
            ViewBag.SubmissionId = model.SubmissionId;
            ViewBag.OrganisationType = model.OrganisationType;

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
    public async Task<IActionResult> SubmitOfflinePayment([FromForm] PaymentDetailsViewModel paymentDetailsViewModel,
        [FromRoute] Guid? submissionId)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);
        if (!GetOrRejectProvidedSubmissionId(submissionId.Value, out var model))
        {
            if (!_currentSession.RegulatorRegistrationSubmissionSession.SelectedOrganisationTypes.TryGetValue(
                    submissionId.Value, out RegistrationSubmissionOrganisationType organisationType))
            {
                return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
            }
            var registrationSubmissionOrganisationDetails = await FetchFromSessionOrFacadeAsync(submissionId.Value, organisationType, _facadeService.GetRegistrationSubmissionDetails);
            model = RegistrationSubmissionDetailsStaticMapper.MapFromOrganisationDetails(registrationSubmissionOrganisationDetails);

        }
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

        return Redirect(Url.RouteUrl("ConfirmOfflinePaymentSubmission", new { submissionId, model.OrganisationType }));
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
            var regulatorDecisionRequest = GetDecisionRequest(existingModel, RegistrationSubmissionStatus.Granted);
            var status = await _facadeService.SubmitRegulatorRegistrationDecisionAsync(regulatorDecisionRequest);

            // this will force a reload of the item in SubmissionDetails
            _currentSession.RegulatorRegistrationSubmissionSession.SelectedRegistrations.Remove(existingModel.SubmissionId);

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
        if (!GetOrRejectProvidedSubmissionId(submissionId.Value, out var model))
        {
            if (!_currentSession.RegulatorRegistrationSubmissionSession.SelectedOrganisationTypes.TryGetValue(
                    submissionId.Value, out RegistrationSubmissionOrganisationType organisationType))
            {
                return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
            }
            var registrationSubmissionOrganisationDetails = await FetchFromSessionOrFacadeAsync(submissionId.Value, organisationType, _facadeService.GetRegistrationSubmissionDetails);
            model = RegistrationSubmissionDetailsStaticMapper.MapFromOrganisationDetails(registrationSubmissionOrganisationDetails);

        }
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
        RegistrationSubmissionDetailsViewModel? regSubmissionDetails = null;
        if (!_currentSession.RegulatorRegistrationSubmissionSession.SelectedOrganisationTypes.TryGetValue(
                model.SubmissionId.Value, out RegistrationSubmissionOrganisationType organisationType))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }
        var registrationSubmissionOrganisationDetails = await FetchFromSessionOrFacadeAsync(model.SubmissionId.Value, organisationType, _facadeService.GetRegistrationSubmissionDetails);
        regSubmissionDetails = RegistrationSubmissionDetailsStaticMapper.MapFromOrganisationDetails(registrationSubmissionOrganisationDetails);

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
    public async Task<IActionResult> RegistrationSubmissionsFileDownload(string downloadType,
        [FromQuery] Guid submissionId)
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
    public async Task<IActionResult> FileDownloadInProgress([FromQuery] Guid submissionId)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);
        RegistrationSubmissionOrganisationDetails submission = null;
        if (!_currentSession.RegulatorRegistrationSubmissionSession.SelectedOrganisationTypes.TryGetValue(
                submissionId, out RegistrationSubmissionOrganisationType organisationType))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        submission = await FetchFromSessionOrFacadeAsync(submissionId, organisationType, _facadeService.GetRegistrationSubmissionDetails);
        if (submission is null)
        {
            _logControllerError.Invoke(logger, $"{submissionId} - submission not found - {nameof(RegistrationSubmissionsController)}.{nameof(RegistrationSubmissions)}", default);
            return RedirectToAction(nameof(PageNotFound));
        }

        var fileDownloadModel = CreateFileDownloadRequest(_currentSession, submission);
        if (fileDownloadModel == null)
        {
            return RedirectToAction(nameof(RegistrationSubmissionFileDownloadFailed), new { submissionId, organisationType });
        }

        var response = await _facadeService.GetFileDownload(fileDownloadModel);

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            return RedirectToAction(nameof(RegistrationSubmissionFileDownloadSecurityWarning), new { submissionId, organisationType });
        }
        else if (response.IsSuccessStatusCode)
        {
            var fileStream = await response.Content.ReadAsStreamAsync();
            var contentDisposition = response.Content.Headers.ContentDisposition;
            var fileName = contentDisposition?.FileNameStar ?? contentDisposition?.FileName ?? submission.SubmissionDetails.Files[0].FileName ?? $"{submission.OrganisationReference}_details.csv";
            TempData["DownloadCompleted"] = true;

            return File(fileStream, "application/octet-stream", fileName);
        }
        else
        {
            return RedirectToAction(nameof(RegistrationSubmissionFileDownloadFailed), new { submissionId, organisationType });
        }
    }

    [HttpGet]
    [Route(PagePath.RegistrationSubmissionFileDownloadFailed)]
    public async Task<IActionResult> RegistrationSubmissionFileDownloadFailed([FromQuery] Guid submissionId, [FromQuery] RegistrationSubmissionOrganisationType organisationType)
    {
        var model = new OrganisationDetailsFileDownloadViewModel(true, false, organisationType, submissionId);
        return View("RegistrationSubmissionFileDownloadFailed", model);
    }

    [HttpGet]
    [Route(PagePath.RegistrationSubmissionFileDownloadSecurityWarning)]
    public async Task<IActionResult> RegistrationSubmissionFileDownloadSecurityWarning([FromQuery] Guid submissionId, [FromQuery] RegistrationSubmissionOrganisationType organisationType)
    {
        var model = new OrganisationDetailsFileDownloadViewModel(true, true, organisationType, submissionId);
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
}