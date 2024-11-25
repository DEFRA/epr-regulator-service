using System.Diagnostics;
using System.Runtime;

using EPR.Common.Authorization.Constants;
using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Extensions;
using EPR.RegulatorService.Frontend.Core.Models;
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

            int nationId = _currentSession.UserData.Organisations[0].NationId != null ? _currentSession.UserData.Organisations[0].NationId.Value : 0;

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
    [Route(PagePath.RegistrationSubmissionDetails + "/{submissionId:guid}", Name = "SubmissionDetails")]
    public async Task<IActionResult> RegistrationSubmissionDetails(Guid? submissionId)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        RegistrationSubmissionDetailsViewModel model = submissionId == null
            ? _currentSession.RegulatorRegistrationSubmissionSession.SelectedRegistration
            :  await FetchFromSessionOrFacadeAsync(submissionId.Value, _facadeService.GetRegistrationSubmissionDetails);

        if (model == null)
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        _currentSession.RegulatorRegistrationSubmissionSession.SelectedRegistration = model;

        GeneratePowerBILink(model);

        SetBackLink(PagePath.RegistrationSubmissionsRoute);
        ViewBag.SubmissionId = model.SubmissionId;

        await SaveSessionAndJourney(_currentSession.RegulatorRegistrationSubmissionSession, PagePath.RegistrationSubmissionsRoute, PagePath.RegistrationSubmissionsRoute);

        return View(nameof(RegistrationSubmissionDetails), model);
    }

    [HttpPost]
    [Route(PagePath.RegistrationSubmissionDetails + "/{submissionId:guid}", Name = "SubmitPaymentInfo")]
    public async Task<IActionResult> SubmitOfflinePayment([FromForm] PaymentDetailsViewModel paymentDetailsViewModel, [FromRoute] Guid? submissionId)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedSubmissionId(submissionId, out var existingModel))
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

        return Redirect(Url.RouteUrl("ConfirmOfflinePaymentSubmission", new { existingModel.SubmissionId }));
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
            SubmissionId = existingModel.SubmissionId
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
            var regulatorDecisionRequest = new RegulatorDecisionRequest
            {
                                    ApplicationReferenceNumber = existingModel.ApplicationReferenceNumber,
                OrganisationId = existingModel.OrganisationId,
                SubmissionId = existingModel.SubmissionId,
                Status = RegistrationSubmissionStatus.Granted.ToString(),
                CountryName = GetCountryCodeInitial(existingModel.NationId),
                RegistrationSubmissionType = existingModel.OrganisationType.GetRegistrationSubmissionType(),
                TwoDigitYear = existingModel.RegistrationYear.Substring(2),
                //TO DO: Refactor existingModel.RegistrationYear.Substring(2) to take from submission date once facade is fixed
                OrganisationAccountManagementId = existingModel.OrganisationReference
            };

            var status = await _facadeService.SubmitRegulatorRegistrationDecisionAsync(regulatorDecisionRequest);

            await UpdateOrganisationDetailsChangeHistoryAsync(existingModel, status, regulatorDecisionRequest);

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

        if (!GetOrRejectProvidedSubmissionId(model.SubmissionId, out RegistrationSubmissionDetailsViewModel existingModel))
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
            var regulatorDecisionRequest = new RegulatorDecisionRequest
            {
                OrganisationId = existingModel.OrganisationId,
                SubmissionId = existingModel.SubmissionId,
                Status = Core.Enums.RegistrationSubmissionStatus.Queried.ToString(),
                Comments = model.Query
            };

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
            SubmissionId = submissionId.Value
        };

        ViewBag.BackToAllSubmissionsUrl = Url.Action("RegistrationSubmissions");

        return View(nameof(RejectRegistrationSubmission), model);
    }

    [HttpPost]
    [Route(PagePath.RejectRegistrationSubmission + "/{submissionId:guid}")]
    public async Task<IActionResult> RejectRegistrationSubmission(RejectRegistrationSubmissionViewModel model)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedSubmissionId(model.SubmissionId, out RegistrationSubmissionDetailsViewModel existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        if (!ModelState.IsValid)
        {
            SetBackLink(Url.RouteUrl("SubmissionDetails", new { model.SubmissionId }), false);
            ViewBag.BackToAllSubmissionsUrl = Url.Action("RegistrationSubmissions");
            return View(nameof(RejectRegistrationSubmission), model);
        }

        _currentSession.RegulatorRegistrationSubmissionSession.SelectedRegistration.RejectReason = model.RejectReason;
        existingModel.RegulatorComments = model.RejectReason;

        SaveSessionAndJourney(
            _currentSession.RegulatorRegistrationSubmissionSession,
            PagePath.RejectRegistrationSubmission,
            PagePath.ConfirmRegistrationRefusal);

        return RedirectToRoute("ConfirmRegistrationRefusal", new { submissionId = existingModel.SubmissionId });
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

        _currentSession.RegulatorRegistrationSubmissionSession.SelectedRegistration.CancellationReason = model.CancellationReason;

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

        if (!GetOrRejectProvidedSubmissionId(submissionId, out var existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        if (string.IsNullOrEmpty(existingModel.PaymentDetails.OfflinePayment))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        SetBackLink(Url.RouteUrl("SubmissionDetails", new { submissionId }), false);

        var model = new ConfirmOfflinePaymentSubmissionViewModel
        {
            SubmissionId = submissionId,
            OfflinePaymentAmount = existingModel.PaymentDetails.OfflinePayment
        };

        return View(nameof(ConfirmOfflinePaymentSubmission), model);
    }

    [HttpPost]
    [Route(PagePath.ConfirmOfflinePaymentSubmission + "/{submissionId:guid}", Name = "ConfirmOfflinePaymentSubmission")]
    public async Task<IActionResult> ConfirmOfflinePaymentSubmission(ConfirmOfflinePaymentSubmissionViewModel model)
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!GetOrRejectProvidedSubmissionId(model.SubmissionId, out var existingModel))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        if (!ModelState.IsValid)
        {
            SetBackLink(Url.RouteUrl("SubmissionDetails", new { model.SubmissionId }), false);
            return View(nameof(ConfirmOfflinePaymentSubmission), model);
        }

        return string.IsNullOrWhiteSpace(model.OfflinePaymentAmount)
            ? RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions")
            : await ProcessOfflinePaymentAsync(existingModel, model.OfflinePaymentAmount);
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

        if (string.IsNullOrEmpty(model.RejectReason))
        {
            return RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions");
        }

        try
        {
            var regulatorDecisionRequest = new RegulatorDecisionRequest
            {
                OrganisationId = existingModel.OrganisationId,
                SubmissionId = existingModel.SubmissionId,
                Status = RegistrationSubmissionStatus.Refused.ToString(),
                Comments = existingModel.RejectReason
            };

            var status = await _facadeService.SubmitRegulatorRegistrationDecisionAsync(regulatorDecisionRequest);

            await UpdateOrganisationDetailsChangeHistoryAsync(existingModel, status, regulatorDecisionRequest);

            return status == Core.Models.EndpointResponseStatus.Success
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
                $"Exception received while refusing submission" +
                $"{nameof(RegistrationSubmissionsController)}.{nameof(ConfirmRegistrationRefusal)}", ex);

            return RedirectToRoute(
                "ServiceNotAvailable",
                new
                {
                    backLink = $"{PagePath.RegistrationSubmissionDetails}/{existingModel.SubmissionId}"
                });
        }

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

        if (string.IsNullOrWhiteSpace(existingModel.CancellationReason))
        {
            return RedirectToRoute("CancelRegistrationSubmission", new { submissionId = existingModel.SubmissionId });
        }

        try
        {
            var regulatorDecisionRequest = new RegulatorDecisionRequest
            {
                OrganisationId = existingModel.OrganisationId,
                SubmissionId = existingModel.SubmissionId,
                Status = RegistrationSubmissionStatus.Cancelled.ToString(),
                Comments = existingModel.CancellationReason,
                DecisionDate = model.CancellationDate
            };

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
    [Route(PagePath.PageNotFoundPath)]
    public async Task<IActionResult> PageNotFound()
    {
        _currentSession = await _sessionManager.GetSessionAsync(HttpContext.Session);

        _currentSession.RegulatorRegistrationSubmissionSession.CurrentPageNumber = 1;

        await SaveSessionAndJourney(_currentSession.RegulatorRegistrationSubmissionSession, PagePath.RegistrationSubmissionsRoute, PagePath.PageNotFound);
        return RedirectToAction(PagePath.Error, "Error", new { statusCode = 404, backLink = PagePath.RegistrationSubmissionsRoute });
    }
}
