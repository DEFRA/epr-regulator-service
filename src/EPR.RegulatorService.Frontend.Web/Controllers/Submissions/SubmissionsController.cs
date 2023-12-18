using Microsoft.AspNetCore.Mvc;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;
using EPR.RegulatorService.Frontend.Core.Extensions;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
using Microsoft.AspNetCore.Authorization;
using EPR.RegulatorService.Frontend.Web.ViewModels.Submissions;
using EPR.Common.Authorization.Constants;
using System.Text.Json;
using EPR.RegulatorService.Frontend.Web.Configs;
using System.Globalization;
using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Services;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement.Mvc;
using RegulatorDecision = EPR.RegulatorService.Frontend.Core.Enums.RegulatorDecision;

namespace EPR.RegulatorService.Frontend.Web.Controllers.Submissions
{
    [FeatureGate(FeatureFlags.ManagePoMSubmissions)]
    [Authorize(Policy = PolicyConstants.RegulatorBasicPolicy)]
    public class SubmissionsController : Controller
    {
        private readonly ISessionManager<JourneySession> _sessionManager;
        private readonly string _pathBase;
        private readonly ExternalUrlsOptions _options;
        private readonly IFacadeService _facadeService;

        public SubmissionsController(ISessionManager<JourneySession> sessionManager,
            IConfiguration configuration, IOptions<ExternalUrlsOptions> options, IFacadeService facadeService)
        {
            _sessionManager = sessionManager;
            _pathBase = configuration.GetValue<string>(ConfigKeys.PathBase);
            _options = options.Value;
            _facadeService = facadeService;
        }

        [HttpGet]
        [Consumes("application/json")]
        [Route(PagePath.Submissions)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Submissions(
            SubmissionFiltersModel submissionFiltersModel,
            EndpointResponseStatus? acceptSubmissionResult = null,
            EndpointResponseStatus? rejectSubmissionResult = null,
            string? organisationName = null,
            int? pageNumber = null)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            session ??= new JourneySession();
            session.RegulatorSubmissionSession.RejectSubmissionJourneyData = null;
            session.RegulatorSubmissionSession.SubmissionFiltersModel ??= new SubmissionFiltersModel();

            SetCustomBackLink();
            SetOrResetFilterValuesInSession(session, submissionFiltersModel, pageNumber);

            var model = new SubmissionsViewModel
            {
                SubmissionFilters = new SubmissionFiltersModel
                {
                    SearchOrganisationName = session.RegulatorSubmissionSession.SubmissionFiltersModel.SearchOrganisationName,
                    SearchOrganisationId = session.RegulatorSubmissionSession.SubmissionFiltersModel.SearchOrganisationId,
                    IsDirectProducerChecked = session.RegulatorSubmissionSession.SubmissionFiltersModel.IsDirectProducerChecked,
                    IsComplianceSchemeChecked = session.RegulatorSubmissionSession.SubmissionFiltersModel.IsComplianceSchemeChecked,
                    IsPendingSubmissionChecked = session.RegulatorSubmissionSession.SubmissionFiltersModel.IsPendingSubmissionChecked,
                    IsAcceptedSubmissionChecked = session.RegulatorSubmissionSession.SubmissionFiltersModel.IsAcceptedSubmissionChecked,
                    IsRejectedSubmissionChecked = session.RegulatorSubmissionSession.SubmissionFiltersModel.IsRejectedSubmissionChecked
                },
                PageNumber =  session.RegulatorSubmissionSession.PageNumber,
                PowerBiLogin = _options.PowerBiLogin,
                AcceptSubmissionResult = acceptSubmissionResult,
                RejectSubmissionResult = rejectSubmissionResult,
                OrganisationName = organisationName
            };

            await SaveSessionAndJourney(session, PagePath.Submissions, PagePath.Submissions);

            return View(model);
        }

        [HttpPost]
        [Route(PagePath.Submissions)]
        public async Task<IActionResult> Submissions(string jsonSubmission)
        {
            var submission = JsonSerializer.Deserialize<Submission>(jsonSubmission);
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            session ??= new JourneySession();
            session.RegulatorSubmissionSession.OrganisationSubmission = submission;

            await SaveSession(session);
            return RedirectToAction("SubmissionDetails", "Submissions");
        }

        [HttpGet]
        [Route(PagePath.SubmissionDetails)]
        public async Task<IActionResult> SubmissionDetails()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            var submission = session.RegulatorSubmissionSession.OrganisationSubmission;

            var model = new SubmissionDetailsViewModel
            {
                OrganisationName = submission.OrganisationName,
                OrganisationType = submission.OrganisationType,
                OrganisationReferenceNumber = submission.OrganisationReference,
                FormattedTimeAndDateOfSubmission = FormatTimeAndDateForSubmission(submission.SubmittedDate),
                SubmissionId = submission.SubmissionId,
                SubmittedBy = $"{submission.FirstName} {submission.LastName}",
                AccountRole = submission.ServiceRole,
                Telephone = submission.Telephone,
                Email = submission.Email,
                Status = submission.Decision,
                IsResubmission = submission.IsResubmission,
                RejectionReason = submission.Comments,
                ResubmissionRequired = submission.IsResubmissionRequired,
                PowerBiLogin = _options.PowerBiLogin
            };

            await SaveSessionAndJourney(session, PagePath.Submissions, PagePath.SubmissionDetails);
            SetBackLink(session, PagePath.SubmissionDetails);

            return View(nameof(SubmissionDetails), model);
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

            return View(nameof(AcceptSubmission) ,model);
        }

        [HttpPost]
        [Route(PagePath.AcceptSubmission)]
        public async Task<IActionResult> AcceptSubmission(AcceptSubmissionViewModel model)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

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
                    OrganisationName = session.RegulatorSubmissionSession.OrganisationSubmission.OrganisationName,
                    OrganisationNumber = session.RegulatorSubmissionSession.OrganisationSubmission.OrganisationReference,
                    IsResubmissionRequired = false
                };
                var result = await _facadeService.SubmitPoMDecision(request);

                return await SaveSessionAndRedirect(
                    session,
                    nameof(Submissions),
                    PagePath.AcceptSubmission,
                    PagePath.Submissions,
                    new
                    {
                        acceptSubmissionResult = result,
                        organisationName = model.OrganisationName
                    });
            }

            return RedirectToAction("SubmissionDetails", "Submissions");
        }

        [HttpPost]
        [Route(PagePath.PreSubmissionDecision)]
        public async Task<IActionResult> PreSubmissionDecision(
            string? organisationName,
            Guid submissionId,
            string? submittedBy)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            session.RegulatorSubmissionSession.RejectSubmissionJourneyData = new RejectSubmissionJourneyData
            {
                OrganisationName = organisationName,
                SubmissionId = submissionId,
                SubmittedBy = submittedBy
            };

            await SaveSession(session);
            return RedirectToAction("RejectSubmission");
        }

        [HttpGet]
        [Route(PagePath.RejectSubmission)]
        public async Task<IActionResult> RejectSubmission()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            var rejectSubmissionJourneyData = session.RegulatorSubmissionSession.RejectSubmissionJourneyData;
            var model = new RejectSubmissionViewModel
            {
                OrganisationName = rejectSubmissionJourneyData.OrganisationName,
                SubmissionId = rejectSubmissionJourneyData.SubmissionId,
                SubmittedBy = rejectSubmissionJourneyData.SubmittedBy
            };

            await SaveSessionAndJourney(session, PagePath.SubmissionDetails, PagePath.RejectSubmission);
            SetBackLink(session, PagePath.RejectSubmission);

            return View(nameof(RejectSubmission), model);
        }

        [HttpGet]
        [Route(PagePath.PrePageNotFound)]
        public async Task<IActionResult> PrePageNotFound()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

            session.RegulatorSubmissionSession.PageNumber = 1;

            SetBackLink(session, PagePath.Submissions);

            return await SaveSessionAndRedirect(session, PagePath.Error, "Error", PagePath.Submissions,
                PagePath.PageNotFoundPath, new {statusCode = 404, backLink = PagePath.Submissions});
        }

        [HttpPost]
        [Route(PagePath.RejectSubmission)]
        public async Task<IActionResult> RejectSubmission(RejectSubmissionViewModel model)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

            if (!ModelState.IsValid)
            {
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
                OrganisationName = session.RegulatorSubmissionSession.OrganisationSubmission.OrganisationName,
                OrganisationNumber = session.RegulatorSubmissionSession.OrganisationSubmission.OrganisationReference,
                IsResubmissionRequired = model.IsResubmissionRequired
            };

            var result = await _facadeService.SubmitPoMDecision(request);

            return await SaveSessionAndRedirect(
                session,
                nameof(Submissions),
                PagePath.RejectSubmission,
                PagePath.Submissions,
                new
                {
                    rejectSubmissionResult = result,
                    organisationName = model.OrganisationName
                });
        }

        public string FormatTimeAndDateForSubmission(DateTime timeAndDateOfSubmission)
        {
            string time = timeAndDateOfSubmission.ToString("h:mm", CultureInfo.CurrentCulture);
            string ampm = timeAndDateOfSubmission.ToString("tt", CultureInfo.CurrentCulture).ToLower();
            string date = timeAndDateOfSubmission.ToString("dd MMMM yyyy", CultureInfo.CurrentCulture);
            return $"{time}{ampm} on {date}";
        }

        private void SetBackLink(JourneySession session, string currentPagePath) =>
            ViewBag.BackLinkToDisplay =
                session.RegulatorSubmissionSession.Journey.PreviousOrDefault(currentPagePath) ?? string.Empty;

        private static void ClearFilters(JourneySession session)
        {
            session.RegulatorSubmissionSession.PageNumber = 1;
            session.RegulatorSubmissionSession.SubmissionFiltersModel.SearchOrganisationName = string.Empty;
            session.RegulatorSubmissionSession.SubmissionFiltersModel.SearchOrganisationId = string.Empty;
            session.RegulatorSubmissionSession.SubmissionFiltersModel.IsDirectProducerChecked = false;
            session.RegulatorSubmissionSession.SubmissionFiltersModel.IsComplianceSchemeChecked = false;
            session.RegulatorSubmissionSession.SubmissionFiltersModel.IsPendingSubmissionChecked = false;
            session.RegulatorSubmissionSession.SubmissionFiltersModel.IsAcceptedSubmissionChecked = false;
            session.RegulatorSubmissionSession.SubmissionFiltersModel.IsRejectedSubmissionChecked = false;
        }

        private static void SetOrResetFilterValuesInSession(JourneySession session, SubmissionFiltersModel submissionFiltersModel, int? pageNumber)
        {
            var regulatorSubmissionSession = session.RegulatorSubmissionSession;
            if (submissionFiltersModel.ClearFilters)
            {
                ClearFilters(session);
            }
            else
            {
                if (IsFilterable(submissionFiltersModel))
                {
                    regulatorSubmissionSession.SubmissionFiltersModel.SearchOrganisationName = submissionFiltersModel.SearchOrganisationName;
                    regulatorSubmissionSession.SubmissionFiltersModel.SearchOrganisationId = submissionFiltersModel.SearchOrganisationId;
                    regulatorSubmissionSession.SubmissionFiltersModel.IsDirectProducerChecked = submissionFiltersModel.IsDirectProducerChecked;
                    regulatorSubmissionSession.SubmissionFiltersModel.IsComplianceSchemeChecked = submissionFiltersModel.IsComplianceSchemeChecked;
                    regulatorSubmissionSession.SubmissionFiltersModel.IsPendingSubmissionChecked = submissionFiltersModel.IsPendingSubmissionChecked;
                    regulatorSubmissionSession.SubmissionFiltersModel.IsAcceptedSubmissionChecked = submissionFiltersModel.IsAcceptedSubmissionChecked;
                    regulatorSubmissionSession.SubmissionFiltersModel.IsRejectedSubmissionChecked = submissionFiltersModel.IsRejectedSubmissionChecked;
                    regulatorSubmissionSession.PageNumber = 1;
                }

                if (pageNumber == null)
                {
                    regulatorSubmissionSession.PageNumber ??= 1;
                }
                else
                {
                    regulatorSubmissionSession.PageNumber = pageNumber;
                }
            }
        }

        private static bool IsFilterable(SubmissionFiltersModel submissionFiltersModel) =>
            (
                (!string.IsNullOrEmpty(submissionFiltersModel.SearchOrganisationName) ||
                 submissionFiltersModel.IsDirectProducerChecked ||
                 submissionFiltersModel.IsComplianceSchemeChecked ||
                 submissionFiltersModel.IsPendingSubmissionChecked ||
                 submissionFiltersModel.IsAcceptedSubmissionChecked ||
                 submissionFiltersModel.IsRejectedSubmissionChecked)
             || submissionFiltersModel.IsFilteredSearch);

        private async Task<RedirectToActionResult> SaveSessionAndRedirect(
            JourneySession session,
            string actionName,
            string currentPagePath,
            string? nextPagePath,
            object? routeValues)
        {
            await SaveSessionAndJourney(session, currentPagePath, nextPagePath);

            return RedirectToAction(actionName, routeValues);
        }

        private async Task<RedirectToActionResult> SaveSessionAndRedirect(
            JourneySession session,
            string actionName,
            string controllerName,
            string currentPagePath,
            string? nextPagePath,
            object? routeValues)
        {
            await SaveSessionAndJourney(session, currentPagePath, nextPagePath);

            return RedirectToAction(actionName, controllerName, routeValues);
        }

        private async Task SaveSessionAndJourney(JourneySession session, string currentPagePath, string? nextPagePath)
        {
            ClearRestOfJourney(session, currentPagePath);

            session.RegulatorSubmissionSession.Journey.AddIfNotExists(nextPagePath);

            await SaveSession(session);
        }

        private void ClearRestOfJourney(JourneySession session, string currentPagePath)
        {
            var index = session.RegulatorSubmissionSession.Journey.IndexOf(currentPagePath);

            // this also cover if current page not found (index = -1) then it clears all pages
            session.RegulatorSubmissionSession.Journey = session.RegulatorSubmissionSession.Journey.Take(index + 1).ToList();
        }

        private async Task SaveSession(JourneySession session) =>
            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

        private void SetCustomBackLink()
        {
            string pathBase = _pathBase.TrimStart('/').TrimEnd('/');
            ViewBag.CustomBackLinkToDisplay = $"/{pathBase}/{PagePath.Home}";
        }
    }
}