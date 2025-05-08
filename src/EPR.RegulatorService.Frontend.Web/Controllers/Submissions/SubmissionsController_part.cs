namespace EPR.RegulatorService.Frontend.Web.Controllers.Submissions
{
    using System.Globalization;

    using EPR.RegulatorService.Frontend.Core.Enums;
    using EPR.RegulatorService.Frontend.Core.Extensions;
    using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
    using EPR.RegulatorService.Frontend.Core.Models;
    using EPR.RegulatorService.Frontend.Core.Models.Submissions;
    using EPR.RegulatorService.Frontend.Core.Sessions;
    using EPR.RegulatorService.Frontend.Web.Constants;
    using EPR.RegulatorService.Frontend.Web.Helpers;

    using Microsoft.AspNetCore.Mvc;
    using System.Reflection;

    public partial class SubmissionsController
    {
        public string FormatTimeAndDateForSubmission(DateTime timeAndDateOfSubmission)
        {
            string time = timeAndDateOfSubmission.ToString("h:mm", CultureInfo.CurrentCulture);
            string ampm = timeAndDateOfSubmission.ToString("tt", CultureInfo.CurrentCulture).ToLower(System.Globalization.CultureInfo.InvariantCulture);
            string date = timeAndDateOfSubmission.ToString("dd MMMM yyyy", CultureInfo.CurrentCulture);
            return $"{time}{ampm} on {date}";
        }

        public void SetOrResetFilterValuesInSession(JourneySession session, SubmissionFiltersModel submissionFiltersModel)
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
                    regulatorSubmissionSession.SearchOrganisationName = submissionFiltersModel.SearchOrganisationName;
                    regulatorSubmissionSession.SearchOrganisationId = submissionFiltersModel.SearchOrganisationId;
                    regulatorSubmissionSession.IsDirectProducerChecked = submissionFiltersModel.IsDirectProducerChecked;
                    regulatorSubmissionSession.IsComplianceSchemeChecked = submissionFiltersModel.IsComplianceSchemeChecked;
                    regulatorSubmissionSession.IsPendingSubmissionChecked = submissionFiltersModel.IsPendingSubmissionChecked;
                    regulatorSubmissionSession.IsAcceptedSubmissionChecked = submissionFiltersModel.IsAcceptedSubmissionChecked;
                    regulatorSubmissionSession.IsRejectedSubmissionChecked = submissionFiltersModel.IsRejectedSubmissionChecked;
                    regulatorSubmissionSession.SearchSubmissionYears = submissionFiltersModel.SearchSubmissionYears;
                    regulatorSubmissionSession.SearchSubmissionPeriods = submissionFiltersModel.SearchSubmissionPeriods;
                    regulatorSubmissionSession.CurrentPageNumber = 1;
                }
            }
        }

        private void SetBackLink(JourneySession session, string currentPagePath) =>
            ViewBag.BackLinkToDisplay =
                session.RegulatorSubmissionSession.Journey.PreviousOrDefault(currentPagePath) ?? string.Empty;

        private void SetBackLink(string path)
        {
            string pathBase = _pathBase.TrimStart('/').TrimEnd('/');
            ViewBag.BackLinkToDisplay = $"/{pathBase}/{path}";
        }

        private static void ClearFilters(JourneySession session)
        {
            session.RegulatorSubmissionSession.CurrentPageNumber = 1;
            session.RegulatorSubmissionSession.SearchOrganisationName = string.Empty;
            session.RegulatorSubmissionSession.SearchOrganisationId = string.Empty;
            session.RegulatorSubmissionSession.IsDirectProducerChecked = false;
            session.RegulatorSubmissionSession.IsComplianceSchemeChecked = false;
            session.RegulatorSubmissionSession.IsPendingSubmissionChecked = false;
            session.RegulatorSubmissionSession.IsAcceptedSubmissionChecked = false;
            session.RegulatorSubmissionSession.IsRejectedSubmissionChecked = false;
            session.RegulatorSubmissionSession.SearchSubmissionYears = Array.Empty<int>();
            session.RegulatorSubmissionSession.SearchSubmissionPeriods = Array.Empty<string>();
        }

        private static bool IsFilterable(SubmissionFiltersModel submissionFiltersModel) =>

                !string.IsNullOrEmpty(submissionFiltersModel.SearchOrganisationName) ||
                 submissionFiltersModel.IsDirectProducerChecked ||
                 submissionFiltersModel.IsComplianceSchemeChecked ||
                 submissionFiltersModel.IsPendingSubmissionChecked ||
                 submissionFiltersModel.IsAcceptedSubmissionChecked ||
                 submissionFiltersModel.IsRejectedSubmissionChecked
             || submissionFiltersModel.SearchSubmissionYears?.Length > 0
             || submissionFiltersModel.SearchSubmissionPeriods?.Length > 0
             || submissionFiltersModel.IsFilteredSearch;

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

        private static void ClearRestOfJourney(JourneySession session, string currentPagePath)
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

        private ViewModels.Submissions.SubmissionDetailsViewModel GetSubmissionDetailsViewModel(JourneySession session, int submissionId)
        {
            var submission = session.RegulatorSubmissionSession.OrganisationSubmissions[submissionId];
            var model = new ViewModels.Submissions.SubmissionDetailsViewModel
            {
                OrganisationName = submission.OrganisationName,
                OrganisationType = submission.OrganisationType,
                OrganisationReferenceNumber = submission.OrganisationReference,
                FormattedTimeAndDateOfSubmission = DateTimeHelpers.FormatTimeAndDateForSubmission(submission.SubmittedDate),
                SubmissionId = submission.SubmissionId,
                SubmissionHash = submissionId,
                SubmittedBy = $"{submission.FirstName} {submission.LastName}",
                SubmissionPeriod = submission.ActualSubmissionPeriod,
                AccountRole = submission.ServiceRole,
                Telephone = submission.Telephone,
                Email = submission.Email,
                Status = submission.Decision,
                IsResubmission = submission.IsResubmission,
                RejectionReason = submission.Comments,
                ResubmissionRequired = submission.IsResubmissionRequired,
                PowerBiLogin = _externalUrlsOptions.PowerBiLogin,
                PreviousRejectionComments = submission.PreviousRejectionComments,
                SubmittedDate = submission.SubmittedDate,
                SubmissionFileName = submission.PomFileName,
                SubmissionBlobName = submission.PomBlobName,
                NationCode = submission.NationCode,
                ReferenceNumber = submission.ReferenceNumber,
                MemberCount = submission.MemberCount,
                ComplianceSchemeId = submission.ComplianceSchemeId
            };

            return model;
        }

        private async Task<IActionResult> ProcessOfflinePaymentAsync(
            string nationCode,
            string referenceNumber,
            string offlinePaymentAmount,
            Guid userId,
            Guid submissionId,
            int submissionHash)
        {
            var response = await _paymentFacadeService.SubmitOfflinePaymentAsync(new OfflinePaymentRequest
            {
                Amount = (int)(decimal.Parse(offlinePaymentAmount, CultureInfo.InvariantCulture) * 100),
                Description = "Packaging data resubmission fee",
                Reference = referenceNumber,
                Regulator = nationCode,
                UserId = userId
            });

            if (response == EndpointResponseStatus.Fail)
            {
                return RedirectToRoute("ServiceNotAvailable", new { backLink = $"{PagePath.SubmissionDetails}?SubmissionId={submissionHash}" });
            }

            await _facadeService.SubmitPackagingDataResubmissionFeePaymentEventAsync(new FeePaymentRequest
            {
                PaidAmount = offlinePaymentAmount,
                PaymentMethod = "Offline",
                PaymentStatus = "Paid",
                SubmissionId = submissionId,
                UserId = userId
            });

            return RedirectToAction("SubmissionDetails", "Submissions", new { SubmissionId = submissionHash });
        }
    }
}
