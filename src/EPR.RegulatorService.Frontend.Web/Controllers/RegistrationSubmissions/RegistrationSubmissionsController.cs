using System.Diagnostics;

using EPR.Common.Authorization.Constants;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
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
                ISessionManager<JourneySession> sessionManager,
                ILogger<RegistrationSubmissionsController> logger,
                IConfiguration configuration,
                IOptions<ExternalUrlsOptions> externalUrlsOptions
             ) : Controller
{
    private readonly string _pathBase = configuration.GetValue<string>(ConfigKeys.PathBase);
    private readonly ExternalUrlsOptions _externalUrlsOptions = externalUrlsOptions.Value;
    private readonly ISessionManager<JourneySession> _sessionManager = sessionManager ?? new JourneySessionManager();
    private readonly IFacadeService _facadeService = facade;
    private JourneySession _currentSession;

    public ISessionManager<JourneySession> SessionManager => _sessionManager;

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

            InitialiseOrContinuePaging(_currentSession.RegulatorRegistrationSubmissionSession, pageNumber);

            ViewBag.PowerBiLogin = _externalUrlsOptions.PowerBiLogin;

            SetBacklinkToHome();

            var viewModel = InitialiseOrCreateViewModel(_currentSession.RegulatorRegistrationSubmissionSession);

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
    [Route(PagePath.RegistrationSubmissionDetails + "/{organisationId:guid}", Name = "SubmissionDetails")]
    public async Task<IActionResult> RegistrationSubmissionDetails(Guid? organisationId)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetAndRememberOrganisationDetails(organisationId, out var model))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        GeneratePowerBILink(model);

        SetBackLink(PagePath.RegistrationSubmissionsRoute);
        ViewBag.OrganisationId = model.OrganisationId;

        await SaveSessionAndJourney(_currentSession.RegulatorRegistrationSubmissionSession, PagePath.RegistrationSubmissionsRoute, PagePath.RegistrationSubmissionsRoute);

        return View(nameof(RegistrationSubmissionDetails), model);
    }

    [HttpPost]
    [Route(PagePath.RegistrationSubmissionDetails + "/{organisationId:guid}", Name = "SubmitPaymentInfo")]
    public async Task<IActionResult> SubmitOfflinePayment([FromForm] PaymentDetailsViewModel paymentDetailsViewModel, [FromRoute] Guid? organisationid)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedOrganisationId(organisationid, out var existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        if (!ModelState.IsValid)
        {
            return View(nameof(RegistrationSubmissionDetails), existingModel);
        }

        paymentDetailsViewModel.EnsureTwoDecimalPlaces();

        existingModel.PaymentDetails.OfflinePayment = paymentDetailsViewModel.OfflinePayment;

        _currentSession.RegulatorRegistrationSubmissionSession.SelectedRegistration = existingModel;

        await SaveSessionAndJourney(
            _currentSession.RegulatorRegistrationSubmissionSession,
            PagePath.RegistrationSubmissionsRoute,
            PagePath.RegistrationSubmissionsRoute);

        return Redirect(Url.RouteUrl("ConfirmOfflinePaymentSubmission", new { existingModel.OrganisationId }));
    }

    [HttpGet]
    [Route(PagePath.GrantRegistrationSubmission + "/{organisationId:guid}", Name = "GrantRegistrationSubmission")]
    public async Task<IActionResult> GrantRegistrationSubmission(Guid? organisationId)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedOrganisationId(organisationId, out var existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        SetBackLink($"{PagePath.RegistrationSubmissionDetails}/{existingModel.OrganisationId}");

        var model = new GrantRegistrationSubmissionViewModel
        {
            OrganisationId = existingModel.OrganisationId
        };

        ViewBag.BackToAllSubmissionsUrl = Url.Action("RegistrationSubmissions");

        return View(nameof(GrantRegistrationSubmission), model);
    }

    [HttpPost]
    [Route(PagePath.GrantRegistrationSubmission + "/{organisationId:guid}", Name = "GrantRegistrationSubmission")]
    public async Task<IActionResult> GrantRegistrationSubmission(GrantRegistrationSubmissionViewModel model)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedOrganisationId(model.OrganisationId, out var existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        if (!ModelState.IsValid && model.IsGrantRegistrationConfirmed == null)
        {
            SetBackLink($"{PagePath.RegistrationSubmissionDetails}/{existingModel.OrganisationId}");
            return View(nameof(GrantRegistrationSubmission), model);
        }
        else if (!model.IsGrantRegistrationConfirmed.Value)
        {
            return RedirectToRoute("SubmissionDetails", new { existingModel.OrganisationId });
        }

        try
        {
            var status = await _facadeService.SubmitRegulatorRegistrationDecisionAsync(
                                new RegulatorDecisionRequest { OrganisationId = existingModel.OrganisationId, Decision = Core.Enums.RegulatorDecision.Accepted });
            return status == Core.Models.EndpointResponseStatus.Success
                  ? RedirectToRoute("SubmissionDetails", new { existingModel.OrganisationId })
                  : RedirectToRoute("ServiceNotAvailable", new { backLink = $"{PagePath.RegistrationSubmissionDetails}/{existingModel.OrganisationId}" });
        }
        catch (Exception ex)
        {
            _logControllerError.Invoke(logger, $"Exception received while granting submission {nameof(RegistrationSubmissionsController)}.{nameof(GrantRegistrationSubmission)}", ex);
            return RedirectToRoute("ServiceNotAvailable", new { backLink = $"{PagePath.RegistrationSubmissionDetails}/{existingModel.OrganisationId}" });
        }
    }

    [HttpGet]
    [Route(PagePath.QueryRegistrationSubmission + "/{organisationId:guid}")]
    public async Task<IActionResult> QueryRegistrationSubmission(Guid? organisationId)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedOrganisationId(organisationId, out RegistrationSubmissionDetailsViewModel existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        SetBackLink(Url.RouteUrl("SubmissionDetails", new { organisationId }), false);

        var model = new QueryRegistrationSubmissionViewModel
        {
            OrganisationId = organisationId.Value
        };

        ViewBag.BackToAllSubmissionsUrl = Url.Action("RegistrationSubmissions");

        return View(nameof(QueryRegistrationSubmission), model);
    }

    [HttpPost]
    [Route(PagePath.QueryRegistrationSubmission + "/{organisationId:guid}")]
    public async Task<IActionResult> QueryRegistrationSubmission(QueryRegistrationSubmissionViewModel model)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedOrganisationId(model.OrganisationId, out RegistrationSubmissionDetailsViewModel existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }
        if (!ModelState.IsValid)
        {
            SetBackLink(Url.RouteUrl("SubmissionDetails", new { model.OrganisationId }), false);
            return View(nameof(QueryRegistrationSubmission), model);
        }

        return RedirectToAction(PagePath.RegistrationSubmissionsAction);
    }

    [HttpGet]
    [Route(PagePath.RejectRegistrationSubmission + "/{organisationId:guid}", Name = "RejectRegistrationSubmission")]
    public async Task<IActionResult> RejectRegistrationSubmission(Guid? organisationId)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedOrganisationId(organisationId, out RegistrationSubmissionDetailsViewModel existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }
        SetBackLink($"{PagePath.RegistrationSubmissionDetails}/{organisationId}");

        var model = new RejectRegistrationSubmissionViewModel
        {
            OrganisationId = organisationId.Value
        };

        ViewBag.BackToAllSubmissionsUrl = Url.Action("RegistrationSubmissions");

        return View(nameof(RejectRegistrationSubmission), model);
    }

    [HttpPost]
    [Route(PagePath.RejectRegistrationSubmission)]
    public async Task<IActionResult> RejectRegistrationSubmission(RejectRegistrationSubmissionViewModel model)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedOrganisationId(model.OrganisationId, out RegistrationSubmissionDetailsViewModel existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }
        if (!ModelState.IsValid)
        {
            SetBackLink(Url.RouteUrl("SubmissionDetails", new { model.OrganisationId }), false);
            return View(nameof(RejectRegistrationSubmission), model);
        }

        return Redirect(PagePath.RegistrationSubmissionsRoute);
    }

    [HttpGet]
    [Route(PagePath.ConfirmOfflinePaymentSubmission + "/{organisationId:guid}", Name = "ConfirmOfflinePaymentSubmission")]
    public async Task<IActionResult> ConfirmOfflinePaymentSubmission(Guid? organisationId)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedOrganisationId(organisationId, out var existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        if (string.IsNullOrEmpty(existingModel.PaymentDetails.OfflinePayment))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        SetBackLink(Url.RouteUrl("SubmissionDetails", new { organisationId }), false);

        var model = new ConfirmOfflinePaymentSubmissionViewModel
        {
            OrganisationId = organisationId,
            OfflinePaymentAmount = existingModel.PaymentDetails.OfflinePayment
        };

        return View(nameof(ConfirmOfflinePaymentSubmission), model);
    }

    [HttpPost]
    [Route(PagePath.ConfirmOfflinePaymentSubmission + "/{organisationId:guid}", Name = "ConfirmOfflinePaymentSubmission")]
    public async Task<IActionResult> ConfirmOfflinePaymentSubmission(ConfirmOfflinePaymentSubmissionViewModel model)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedOrganisationId(model.OrganisationId, out var existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        if (!ModelState.IsValid)
        {
            SetBackLink(Url.RouteUrl("SubmissionDetails", new { model.OrganisationId }), false);
            return View(nameof(ConfirmOfflinePaymentSubmission), model);
        }

        if (string.IsNullOrEmpty(model.OfflinePaymentAmount))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        // This is where we will call the facade to submit the offline payment.

        return Redirect(Url.RouteUrl("SubmissionDetails", new { model.OrganisationId }));
    }

    [HttpGet]
    [Route(PagePath.CancellationConfirmation + "/{organisationId:guid}")]
    public async Task<IActionResult> CancellationConfirmation(Guid? organisationId)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedOrganisationId(organisationId, out var existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        if (string.IsNullOrEmpty(existingModel.OrganisationName))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        var model = new CancellationConfirmationViewModel
        {
            OrganisationId = existingModel.OrganisationId,
            OrganisationName = existingModel.OrganisationName
        };

        ViewBag.BackToAllSubmissionsUrl = Url.Action("RegistrationSubmissions");

        return View(nameof(CancellationConfirmation), model);
    }

    [HttpGet]
    [Route(PagePath.PageNotFoundPath)]
    public async Task<IActionResult> PageNotFound()
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        _currentSession.RegulatorRegistrationSubmissionSession.CurrentPageNumber = 1;

        await SaveSessionAndJourney(_currentSession.RegulatorRegistrationSubmissionSession, PagePath.RegistrationSubmissionsRoute, PagePath.PageNotFound);
        return RedirectToAction(PagePath.Error, "Error", new { statusCode = 404, backLink = PagePath.RegistrationSubmissionsRoute });
    }
}
