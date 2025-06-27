using System.Globalization;
using System.Linq;
using System.Text.Json;

using EPR.Common.Authorization.Constants;
using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.FileDownload;
using EPR.RegulatorService.Frontend.Core.Models.FileDownload;
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

using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

using RegulatorDecision = EPR.RegulatorService.Frontend.Core.Enums.RegulatorDecision;
using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Models.FileDownload;

namespace EPR.RegulatorService.Frontend.Web.Controllers.Submissions;

[FeatureGate(FeatureFlags.ManagePoMSubmissions)]
[Authorize(Policy = PolicyConstants.RegulatorBasicPolicy)]
public partial class SubmissionsController : Controller
{
    private readonly ISessionManager<JourneySession> _sessionManager;
    private readonly string _pathBase;
    private readonly SubmissionFiltersConfig _submissionFiltersOptions;
    private readonly ExternalUrlsOptions _externalUrlsOptions;
    private readonly IFacadeService _facadeService;
    private readonly ISubmissionFilterConfigService _submissionFilterConfigService;
    private readonly IPaymentFacadeService _paymentFacadeService;
    private const string SubmissionResultAccept = "SubmissionResultAccept";
    private const string SubmissionResultReject = "SubmissionResultReject";
    private const string SubmissionResultOrganisationName = "SubmissionResultOrganisationName";

    public SubmissionsController(
        ISessionManager<JourneySession> sessionManager,
        IConfiguration configuration,
        IOptions<SubmissionFiltersConfig> submissionFiltersConfig,
        IOptions<ExternalUrlsOptions> externalUrlsOptions,
        IFacadeService facadeService,
        ISubmissionFilterConfigService submissionFilterConfigService,
        IPaymentFacadeService paymentFacadeService)
    {
        _sessionManager = sessionManager;
        _pathBase = configuration.GetValue<string>(ConfigKeys.PathBase);
        _submissionFiltersOptions = submissionFiltersConfig.Value;
        _externalUrlsOptions = externalUrlsOptions.Value;
        _facadeService = facadeService;
        _submissionFilterConfigService = submissionFilterConfigService;
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

        var (submissionYears, submissonPeriods) = _submissionFilterConfigService.GetFilteredSubmissionYearsAndPeriods();

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
            SubmissionYears = submissionYears,
            SubmissionPeriods = submissonPeriods,
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

            int hash = RegulatorSubmissionSession.GetSubmissionHashCode(submission);

            session.RegulatorSubmissionSession.OrganisationSubmissions[hash] = submission;

            return await SaveSessionAndRedirect(
                session,
                nameof(SubmissionDetails),
                PagePath.Submissions,
                PagePath.SubmissionDetails,
                new { submissionHash = hash });
        }

        //if filtering
        if (filterType == FilterActions.ClearFilters)
        {
            viewModel.ClearFilters = true;
            submissionFiltersModel.ClearFilters = true;
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
    public async Task<IActionResult> SubmissionDetails([FromQuery] int submissionHash)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        var model = GetSubmissionDetailsViewModel(session, submissionHash);

        if (model.IsResubmission)
        {
            var payCalParameters = await _facadeService.GetPomPayCalParameters(
                        model.SubmissionId,
                        model.ComplianceSchemeId);

            model.ReferenceFieldNotAvailable = model.ReferenceNotAvailable = true;

            var sessionSubmission = session.RegulatorSubmissionSession.OrganisationSubmissions[submissionHash];

            if (payCalParameters is not null)
            {
                sessionSubmission.NationCode
                    = model.NationCode = payCalParameters.NationCode;
                sessionSubmission.ReferenceNumber
                    = model.ReferenceNumber = payCalParameters.Reference;
                sessionSubmission.MemberCount
                    = model.MemberCount = payCalParameters.MemberCount ?? 0;
                model.ReferenceFieldNotAvailable = payCalParameters.ReferenceFieldNotAvailable;
                model.ReferenceNotAvailable = payCalParameters.ReferenceNotAvailable;
                model.SubmittedDate = payCalParameters.ResubmissionDate ?? model.SubmittedDate;
            }
        }

        await SaveSessionAndJourney(session, PagePath.Submissions, PagePath.SubmissionDetails);
        SetBackLink(session, PagePath.SubmissionDetails);

        return View(nameof(SubmissionDetails), model);
    }

    [Route(PagePath.SubmissionDetails, Name = "ResubmissionPaymentInfo")]
    public async Task<IActionResult> SubmitOfflinePayment([FromForm] PaymentDetailsViewModel paymentDetailsViewModel)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.SubmissionDetails);
            var model = GetSubmissionDetailsViewModel(session, paymentDetailsViewModel.SubmissionHash);
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

        return RedirectToAction(
            "ConfirmOfflinePaymentSubmission",
            "Submissions",
            new
            {
                submissionHash = paymentDetailsViewModel.SubmissionHash
            });
    }

    [HttpGet]
    [Route(PagePath.ConfirmOfflinePaymentSubmission)]
    public async Task<IActionResult> ConfirmOfflinePaymentSubmission([FromQuery] int submissionHash)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        var submission = session.RegulatorSubmissionSession.OrganisationSubmissions[submissionHash];

        string offlinePayment = TempData.Peek("OfflinePaymentAmount").ToString();

        SetBackLink($"{PagePath.SubmissionDetails}?submissionHash={submissionHash}");

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
        var submission = session.RegulatorSubmissionSession.OrganisationSubmissions[model.SubmissionHash.Value];

        if (!ModelState.IsValid)
        {
            SetBackLink($"{PagePath.SubmissionDetails}?submissionHash={model.SubmissionHash}");
            return View(nameof(ConfirmOfflinePaymentSubmission), model);
        }
        else if (!(bool)model.IsOfflinePaymentConfirmed)
        {
            return RedirectToAction("SubmissionDetails", "Submissions", new { submissionHash = model.SubmissionHash.Value });
        }

        TempData.Remove("OfflinePaymentAmount");
        return string.IsNullOrWhiteSpace(model.OfflinePaymentAmount)
            ? RedirectToAction(
                PagePath.Error,
                "Error",
                new
                {
                    statusCode = 404,
                    backLink = $"{PagePath.SubmissionDetails}?submissionHash={model.SubmissionHash.Value}"
                })
            : await ProcessOfflinePaymentAsync(
                submission.NationCode,
                submission.ReferenceNumber, // To do: This will be done as part of 517712
                model.OfflinePaymentAmount,
                submission.UserId.Value,
                submission.SubmissionId,
                model.SubmissionHash.Value);
    }

    [HttpGet]
    [Route(PagePath.AcceptSubmission)]
    public async Task<IActionResult> AcceptSubmission([FromQuery] int submissionHash)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        var model = new AcceptSubmissionViewModel
        {
            SubmissionHash = submissionHash,
            OrganisationName = session.RegulatorSubmissionSession.OrganisationSubmissions[submissionHash].OrganisationName
        };

        await SaveSessionAndJourney(session, PagePath.SubmissionDetails, PagePath.AcceptSubmission);
        SetBackLink($"{PagePath.SubmissionDetails}?submissionHash={submissionHash}");

        return View(nameof(AcceptSubmission), model);
    }

    [HttpPost]
    [Route(PagePath.AcceptSubmission)]
    public async Task<IActionResult> AcceptSubmission(AcceptSubmissionViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        var submission = session.RegulatorSubmissionSession.OrganisationSubmissions[model.SubmissionHash];
        string organisationName = submission.OrganisationName;

        if (!ModelState.IsValid)
        {
            SetBackLink($"{PagePath.SubmissionDetails}?submissionHash={model.SubmissionHash}");
            return View(nameof(AcceptSubmission), model);
        }

        if (model.Accepted == true)
        {
            var request = new RegulatorPoMDecisionCreateRequest
            {
                SubmissionId = submission.SubmissionId,
                Decision = RegulatorDecision.Accepted,
                FileId = submission.FileId,
                OrganisationId = submission.OrganisationId,
                OrganisationName = organisationName,
                OrganisationNumber = submission.OrganisationReference,
                IsResubmissionRequired = false,
                SubmissionPeriod = submission.SubmissionPeriod

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

        return RedirectToAction("SubmissionDetails", "Submissions", new { model.SubmissionHash });
    }


    [HttpGet]
    [Route(PagePath.RejectSubmission)]
    public async Task<IActionResult> RejectSubmission([FromQuery] int submissionHash)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        var submission = session.RegulatorSubmissionSession.OrganisationSubmissions[submissionHash];

        session.RegulatorSubmissionSession.RejectSubmissionJourneyData = new RejectSubmissionJourneyData
        {
            SubmittedBy = $"{submission.FirstName} {submission.LastName}"
        };

        var model = new RejectSubmissionViewModel
        {
            SubmissionHash = submissionHash,
            SubmittedBy = session.RegulatorSubmissionSession.RejectSubmissionJourneyData.SubmittedBy
        };

        await SaveSessionAndJourney(session, PagePath.SubmissionDetails, PagePath.RejectSubmission);
        SetBackLink($"{PagePath.SubmissionDetails}?submissionHash={submissionHash}");

        return View(nameof(RejectSubmission), model);
    }


    [HttpPost]
    [Route(PagePath.RejectSubmission)]
    public async Task<IActionResult> RejectSubmission(RejectSubmissionViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        var submission = session.RegulatorSubmissionSession.OrganisationSubmissions[model.SubmissionHash];
        string organisationName = submission.OrganisationName;

        if (!ModelState.IsValid)
        {
            model.SubmittedBy = session.RegulatorSubmissionSession.RejectSubmissionJourneyData.SubmittedBy;

            SetBackLink($"{PagePath.SubmissionDetails}?submissionHash={model.SubmissionHash}");
            return View(nameof(RejectSubmission), model);
        }

        var request = new RegulatorPoMDecisionCreateRequest
        {
            SubmissionId = submission.SubmissionId,
            Decision = RegulatorDecision.Rejected,
            Comments = model.ReasonForRejection,
            FileId = submission.FileId,
            OrganisationId = submission.OrganisationId,
            OrganisationName = organisationName,
            OrganisationNumber = submission.OrganisationReference,
            IsResubmissionRequired = model.IsResubmissionRequired,
            SubmissionPeriod = submission.SubmissionPeriod
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

    [HttpGet]
    [Route(PagePath.SubmissionsFileDownload)]
    public async Task<IActionResult> SubmissionsFileDownload([FromQuery] int submissionHash)
    {
        TempData["DownloadCompleted"] = false;

        return RedirectToAction(nameof(PackagingDataFileDownload), "Submissions", new { submissionHash });
    }


    [HttpGet]
    [Route(PagePath.PackagingDataFileDownload)]
    public IActionResult PackagingDataFileDownload([FromQuery] int submissionHash)
    {
        TempData["SubmissionHash"] = submissionHash;
        return View("PackagingDataFileDownload");
    }


    [HttpGet]
    public async Task<IActionResult> FileDownloadInProgress([FromQuery] int submissionHash)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        var submission = session.RegulatorSubmissionSession.OrganisationSubmissions[submissionHash];
        var fileDownloadModel = CreateFileDownloadRequest(submission);

        if (fileDownloadModel == null)
        {
            return RedirectToAction(nameof(PackagingDataFileDownloadFailed), new { submissionHash });
        }

        var response = await _facadeService.GetFileDownload(fileDownloadModel);

        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            return RedirectToAction(nameof(PackagingDataFileDownloadSecurityWarning), new { submissionHash });
        }
        else if (response.IsSuccessStatusCode)
        {
            var fileStream = await response.Content.ReadAsStreamAsync();
            var contentDisposition = response.Content.Headers.ContentDisposition;
            var fileName = contentDisposition?.FileNameStar ?? contentDisposition?.FileName ?? submission.PomFileName;
            TempData["DownloadCompleted"] = true;

            return File(fileStream, "application/octet-stream", fileName);
        }
        else
        {
            return RedirectToAction(nameof(PackagingDataFileDownloadFailed), new { submissionHash });
        }
    }


    [HttpGet]
    [Route(PagePath.PackagingDataFileDownloadFailed)]
    public IActionResult PackagingDataFileDownloadFailed([FromQuery] int submissionHash)
    {
        var model = new SubmissionDetailsFileDownloadViewModel(true, false) { SubmissionHash = submissionHash };
        return View("PackagingDataFileDownloadFailed", model);
    }

    [HttpGet]
    [Route(PagePath.PackagingDataFileDownloadSecurityWarning)]
    public async Task<IActionResult> PackagingDataFileDownloadSecurityWarning([FromQuery] int submissionHash)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        var submission = session.RegulatorSubmissionSession.OrganisationSubmissions[submissionHash];
        string submittedBy = $"{submission.FirstName} {submission.LastName}";
        var model = new SubmissionDetailsFileDownloadViewModel(true, true, null, submittedBy) { SubmissionHash = submissionHash };
        return View("PackagingDataFileDownloadFailed", model);
    }

    private static FileDownloadRequest CreateFileDownloadRequest(Submission submission)
    {
        var fileDownloadModel = new FileDownloadRequest
        {
            SubmissionId = submission.SubmissionId,
            SubmissionType = SubmissionType.Producer,
            FileId = submission.FileId,
            BlobName = submission.PomBlobName,
            FileName = submission.PomFileName,
        };

        if (fileDownloadModel.FileId == null || fileDownloadModel.BlobName == null || fileDownloadModel.FileName == null)
        {
            return null;
        }

        return fileDownloadModel;
    }

}