using System.Globalization;
using System.Text.Json;

using EPR.Common.Authorization.Constants;
using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.Applications;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Web.ViewModels.Submissions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement.Mvc;

using RegulatorDecision = EPR.RegulatorService.Frontend.Core.Enums.RegulatorDecision;

namespace EPR.RegulatorService.Frontend.Web.Controllers.Submissions;

[FeatureGate(FeatureFlags.ManagePoMSubmissions)]
[Authorize(Policy = PolicyConstants.RegulatorBasicPolicy)]
public partial class SubmissionsController : Controller
{
    private readonly ISessionManager<JourneySession> _sessionManager;
    private readonly string _pathBase;
    private readonly SubmissionFiltersOptions _submissionFiltersOptions;
    private readonly ExternalUrlsOptions _externalUrlsOptions;
    private readonly IFacadeService _facadeService;
    private readonly IPaymentFacadeService _paymentFacadeService;
    private const string SubmissionResultAccept = "SubmissionResultAccept";
    private const string SubmissionResultReject = "SubmissionResultReject";
    private const string SubmissionResultOrganisationName = "SubmissionResultOrganisationName";

    public SubmissionsController(
        ISessionManager<JourneySession> sessionManager,
        IConfiguration configuration,
        IOptions<SubmissionFiltersOptions> submissionFiltersOptions,
        IOptions<ExternalUrlsOptions> externalUrlsOptions,
        IFacadeService facadeService,
        IPaymentFacadeService paymentFacadeService)
    {
        _sessionManager = sessionManager;
        _pathBase = configuration.GetValue<string>(ConfigKeys.PathBase);
        _submissionFiltersOptions = submissionFiltersOptions.Value;
        _externalUrlsOptions = externalUrlsOptions.Value;
        _facadeService = facadeService;
        _paymentFacadeService = paymentFacadeService;
    }

    [HttpGet]
    [Consumes("application/json")]
    [Route(PagePath.Submissions)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Submissions(int? pageNumber = null)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new JourneySession();
        session.RegulatorSubmissionSession.RejectSubmissionJourneyData = null;

        if (pageNumber == null)
        {
            session.RegulatorSubmissionSession.CurrentPageNumber ??= 1;
        }
        else
        {
            session.RegulatorSubmissionSession.CurrentPageNumber = pageNumber;
        }

        SetCustomBackLink();
        EndpointResponseStatus? submissionResultAccept = TempData.TryGetValue(SubmissionResultAccept, out object? acceptSubmissionResult) ? (EndpointResponseStatus)acceptSubmissionResult : EndpointResponseStatus.NotSet;
        EndpointResponseStatus? submissionResultReject = TempData.TryGetValue(SubmissionResultReject, out object? rejectSubmissionResult) ? (EndpointResponseStatus)rejectSubmissionResult : EndpointResponseStatus.NotSet;
        string? submissionResultOrganisationName = TempData.TryGetValue(SubmissionResultOrganisationName, out object? organisationName) ? organisationName.ToString() : string.Empty;

        var model = new SubmissionsViewModel
        {
            SearchSubmissionYears = session.RegulatorSubmissionSession.SearchSubmissionYears,
            SearchSubmissionPeriods = session.RegulatorSubmissionSession.SearchSubmissionPeriods,
            SearchOrganisationName = session.RegulatorSubmissionSession.SearchOrganisationName,
            SearchOrganisationId = session.RegulatorSubmissionSession.SearchOrganisationId,
            IsDirectProducerChecked = session.RegulatorSubmissionSession.IsDirectProducerChecked,
            IsComplianceSchemeChecked = session.RegulatorSubmissionSession.IsComplianceSchemeChecked,
            IsPendingSubmissionChecked = session.RegulatorSubmissionSession.IsPendingSubmissionChecked,
            IsAcceptedSubmissionChecked = session.RegulatorSubmissionSession.IsAcceptedSubmissionChecked,
            IsRejectedSubmissionChecked = session.RegulatorSubmissionSession.IsRejectedSubmissionChecked,
            PageNumber = session.RegulatorSubmissionSession.CurrentPageNumber,
            PowerBiLogin = _externalUrlsOptions.PowerBiLogin,
            SubmissionYears = _submissionFiltersOptions.Years,
            SubmissionPeriods = _submissionFiltersOptions.PomPeriods,
            AcceptSubmissionResult = submissionResultAccept,
            RejectSubmissionResult = submissionResultReject,
            OrganisationName = submissionResultOrganisationName
        };

        await SaveSessionAndJourney(session, PagePath.Submissions, PagePath.Submissions);

        return View(model);
    }

    [HttpPost]
    [Route(PagePath.Submissions)]
    public async Task<IActionResult> Submissions(SubmissionsRequestViewModel viewModel, string? filterType = null, string? jsonSubmission = null, bool? export = null)
    {
        SubmissionFiltersModel submissionFiltersModel = new SubmissionFiltersModel()
        {
            SearchOrganisationName = viewModel.SearchOrganisationName,
            SearchOrganisationId = viewModel.SearchOrganisationId,
            IsDirectProducerChecked = viewModel.IsDirectProducerChecked,
            IsComplianceSchemeChecked = viewModel.IsComplianceSchemeChecked,
            IsPendingSubmissionChecked = viewModel.IsPendingSubmissionChecked,
            IsAcceptedSubmissionChecked = viewModel.IsAcceptedSubmissionChecked,
            IsRejectedSubmissionChecked = viewModel.IsRejectedSubmissionChecked,
            SearchSubmissionYears = viewModel.SearchSubmissionYears?.Where(x => _submissionFiltersOptions.Years.Contains(x)).ToArray(),
            SearchSubmissionPeriods = viewModel.SearchSubmissionPeriods?.Where(x => _submissionFiltersOptions.PomPeriods.Contains(x)).ToArray(),
            IsFilteredSearch = viewModel.IsFilteredSearch,
            ClearFilters = viewModel.ClearFilters
        };

        if (export == true)
        {
            var stream = await _facadeService.GetPackagingSubmissionsCsv(new GetPackagingSubmissionsCsvRequest
            {
                SearchOrganisationName = submissionFiltersModel.SearchOrganisationName,
                SearchOrganisationId = submissionFiltersModel.SearchOrganisationId,
                IsDirectProducerChecked = submissionFiltersModel.IsDirectProducerChecked,
                IsComplianceSchemeChecked = submissionFiltersModel.IsComplianceSchemeChecked,
                IsPendingSubmissionChecked = submissionFiltersModel.IsPendingSubmissionChecked,
                IsAcceptedSubmissionChecked = submissionFiltersModel.IsAcceptedSubmissionChecked,
                IsRejectedSubmissionChecked = submissionFiltersModel.IsRejectedSubmissionChecked,
                SearchSubmissionPeriods = submissionFiltersModel.SearchSubmissionPeriods,
                SearchSubmissionYears = submissionFiltersModel.SearchSubmissionYears
            });

            return File(stream, "text/csv", "packaging-submissions.csv");
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new();

        // if not filtering
        if (filterType == null)
        {
            if (jsonSubmission == null)
            {
                return RedirectToAction(PagePath.Error, "Error");
            }

            var submission = JsonSerializer.Deserialize<Submission>(jsonSubmission);
            session.RegulatorSubmissionSession.OrganisationSubmission = submission;

            return await SaveSessionAndRedirect(
                session,
                nameof(SubmissionDetails),
                PagePath.Submissions,
                PagePath.SubmissionDetails,
                null);
        }

        //if filtering
        if (filterType == FilterActions.ClearFilters)
        {
            viewModel.ClearFilters = true;
        }

        SetOrResetFilterValuesInSession(session, submissionFiltersModel);

        return await SaveSessionAndRedirect(
            session,
            nameof(Submissions),
            PagePath.Submissions,
            PagePath.Submissions,
            null);
    }

    [HttpGet]
    [Route(PagePath.SubmissionDetails)]
    public async Task<IActionResult> SubmissionDetails()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        var model = GetSubmissionDetailsViewModel(session);

        await SaveSessionAndJourney(session, PagePath.Submissions, PagePath.SubmissionDetails);
        SetBackLink(session, PagePath.SubmissionDetails);

        return View(nameof(SubmissionDetails), model);
    }

    [HttpPost]
    [Route(PagePath.SubmissionDetails, Name = "ResubmissionPaymentInfo")]
    public async Task<IActionResult> SubmitOfflinePayment([FromForm] PaymentDetailsViewModel paymentDetailsViewModel)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.SubmissionDetails);
            var model = GetSubmissionDetailsViewModel(session);
            return View(nameof(SubmissionDetails), model);
        }

        if (decimal.TryParse(paymentDetailsViewModel.OfflinePayment, NumberStyles.Currency, CultureInfo.InvariantCulture, out decimal parsedValue))
        {
            paymentDetailsViewModel.OfflinePayment = parsedValue.ToString("F2", CultureInfo.InvariantCulture);
        }

        TempData["OfflinePaymentAmount"] = paymentDetailsViewModel.OfflinePayment;

        await SaveSessionAndJourney(
            session,
            PagePath.SubmissionDetails,
            PagePath.ConfirmOfflinePaymentSubmission);

        return RedirectToAction("ConfirmOfflinePaymentSubmission");
    }

    [HttpGet]
    [Route(PagePath.ConfirmOfflinePaymentSubmission)]
    public async Task<IActionResult> ConfirmOfflinePaymentSubmission()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        var submission = session.RegulatorSubmissionSession.OrganisationSubmission;

        string offlinePayment = TempData.Peek("OfflinePaymentAmount").ToString();

        SetBackLink(PagePath.SubmissionDetails);

        var model = new ConfirmOfflinePaymentSubmissionViewModel
        {
            SubmissionId = submission.SubmissionId,
            OfflinePaymentAmount = offlinePayment
        };

        return View(nameof(ConfirmOfflinePaymentSubmission), model);
    }

    [HttpPost]
    [Route(PagePath.ConfirmOfflinePaymentSubmission)]
    public async Task<IActionResult> ConfirmOfflinePaymentSubmission(ConfirmOfflinePaymentSubmissionViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        var submission = session.RegulatorSubmissionSession.OrganisationSubmission;
        int nationId = session.UserData.Organisations[0].NationId.Value;

        if (!ModelState.IsValid)
        {
            SetBackLink(PagePath.SubmissionDetails);
            return View(nameof(ConfirmOfflinePaymentSubmission), model);
        }
        else if (!(bool)model.IsOfflinePaymentConfirmed)
        {
            return RedirectToAction("SubmissionDetails");
        }

        TempData.Remove("OfflinePaymentAmount");
        return string.IsNullOrWhiteSpace(model.OfflinePaymentAmount)
            ? RedirectToAction(
                PagePath.Error,
                "Error",
                new
                {
                    statusCode = 404,
                    backLink = PagePath.SubmissionDetails
                })
            : await ProcessOfflinePaymentAsync(
                nationId,
                "referenceNumber",
                model.OfflinePaymentAmount,
                submission.UserId.Value,
                submission.SubmissionId);

        //reference, userId, amount, description = packaging data, date (optional), user notes (optional),
        // regulator (GB-ENG, GB-SCT, GB-WLS, GB-NIR)

        // TO DO: We need to call ProcessOfflinePaymentAsync (not yet copied) and somehow pass through the submission ID, applicationReferenceNumber
        // and NationCode from the session objects in order to process the offline payment. This will be addressed in a future story
    }

    [HttpGet]
    [Route(PagePath.AcceptSubmission)]
    public async Task<IActionResult> AcceptSubmission()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        var model = new AcceptSubmissionViewModel
        {
            OrganisationName = session.RegulatorSubmissionSession.OrganisationSubmission.OrganisationName
        };

        await SaveSessionAndJourney(session, PagePath.SubmissionDetails, PagePath.AcceptSubmission);
        SetBackLink(session, PagePath.AcceptSubmission);

        return View(nameof(AcceptSubmission), model);
    }

    [HttpPost]
    [Route(PagePath.AcceptSubmission)]
    public async Task<IActionResult> AcceptSubmission(AcceptSubmissionViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        var organisationName = session.RegulatorSubmissionSession.OrganisationSubmission.OrganisationName;
        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.AcceptSubmission);
            return View(nameof(AcceptSubmission), model);
        }

        if (model.Accepted == true)
        {
            var request = new RegulatorPoMDecisionCreateRequest
            {
                SubmissionId = session.RegulatorSubmissionSession.OrganisationSubmission.SubmissionId,
                Decision = RegulatorDecision.Accepted,
                FileId = session.RegulatorSubmissionSession.OrganisationSubmission.FileId,
                OrganisationId = session.RegulatorSubmissionSession.OrganisationSubmission.OrganisationId,
                OrganisationName = organisationName,
                OrganisationNumber = session.RegulatorSubmissionSession.OrganisationSubmission.OrganisationReference,
                IsResubmissionRequired = false,
                SubmissionPeriod = session.RegulatorSubmissionSession.OrganisationSubmission.SubmissionPeriod

            };
            var result = await _facadeService.SubmitPoMDecision(request);

            TempData[SubmissionResultAccept] = result;
            TempData[SubmissionResultOrganisationName] = organisationName;

            return await SaveSessionAndRedirect(
                session,
                nameof(Submissions),
                PagePath.AcceptSubmission,
                PagePath.Submissions,
                null);
        }

        return RedirectToAction("SubmissionDetails", "Submissions");
    }

    [HttpGet]
    [Route(PagePath.RejectSubmission)]
    public async Task<IActionResult> RejectSubmission()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        session.RegulatorSubmissionSession.RejectSubmissionJourneyData = new RejectSubmissionJourneyData
        {
            SubmittedBy = $"{session.RegulatorSubmissionSession.OrganisationSubmission.FirstName} {session.RegulatorSubmissionSession.OrganisationSubmission.LastName}"
        };

        var model = new RejectSubmissionViewModel
        {
            SubmittedBy = session.RegulatorSubmissionSession.RejectSubmissionJourneyData.SubmittedBy
        };

        await SaveSessionAndJourney(session, PagePath.SubmissionDetails, PagePath.RejectSubmission);
        SetBackLink(session, PagePath.RejectSubmission);

        return View(nameof(RejectSubmission), model);
    }

    [HttpPost]
    [Route(PagePath.RejectSubmission)]
    public async Task<IActionResult> RejectSubmission(RejectSubmissionViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        var organisationName = session.RegulatorSubmissionSession.OrganisationSubmission.OrganisationName;

        if (!ModelState.IsValid)
        {
            model.SubmittedBy = session.RegulatorSubmissionSession.RejectSubmissionJourneyData.SubmittedBy;

            SetBackLink(session, PagePath.RejectSubmission);
            return View(nameof(RejectSubmission), model);
        }

        var request = new RegulatorPoMDecisionCreateRequest
        {
            SubmissionId = session.RegulatorSubmissionSession.OrganisationSubmission.SubmissionId,
            Decision = RegulatorDecision.Rejected,
            Comments = model.ReasonForRejection,
            FileId = session.RegulatorSubmissionSession.OrganisationSubmission.FileId,
            OrganisationId = session.RegulatorSubmissionSession.OrganisationSubmission.OrganisationId,
            OrganisationName = organisationName,
            OrganisationNumber = session.RegulatorSubmissionSession.OrganisationSubmission.OrganisationReference,
            IsResubmissionRequired = model.IsResubmissionRequired,
            SubmissionPeriod = session.RegulatorSubmissionSession.OrganisationSubmission.SubmissionPeriod
        };

        var result = await _facadeService.SubmitPoMDecision(request);

        TempData[SubmissionResultReject] = result;
        TempData[SubmissionResultOrganisationName] = organisationName;

        return await SaveSessionAndRedirect(
            session,
            nameof(Submissions),
            PagePath.RejectSubmission,
            PagePath.Submissions,
            null);
    }

    [HttpGet]
    [Route(PagePath.PrePageNotFound)]
    public async Task<IActionResult> PrePageNotFound()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        session.RegulatorSubmissionSession.CurrentPageNumber = 1;

        SetBackLink(session, PagePath.Submissions);

        return await SaveSessionAndRedirect(session, PagePath.Error, "Error", PagePath.Submissions,
            PagePath.PageNotFoundPath, new { statusCode = 404, backLink = PagePath.Submissions });
    }

}