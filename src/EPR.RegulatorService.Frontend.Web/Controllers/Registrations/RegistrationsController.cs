using System.Text.Json;

using EPR.Common.Authorization.Constants;
using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Extensions;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.FileDownload;
using EPR.RegulatorService.Frontend.Core.Models.Registrations;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Helpers;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.Registrations;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement.Mvc;

using RegulatorDecision = EPR.RegulatorService.Frontend.Core.Enums.RegulatorDecision;

namespace EPR.RegulatorService.Frontend.Web.Controllers.Registrations
{
    [FeatureGate(FeatureFlags.ManageRegistrations)]
    [Authorize(Policy = PolicyConstants.RegulatorBasicPolicy)]
    public class RegistrationsController : Controller
    {
        private const string DeclaredByComplianceScheme = "Not required (compliance scheme)";
        private readonly ISessionManager<JourneySession> _sessionManager;
        private readonly string _pathBase;
        private readonly SubmissionFiltersConfig _submissionFiltersConfig;
        private readonly ExternalUrlsOptions _externalUrlsOptions;
        private readonly IFacadeService _facadeService;

        public RegistrationsController(
            ISessionManager<JourneySession> sessionManager,
            IConfiguration configuration,
            IOptions<SubmissionFiltersConfig> submissionFiltersConfig,
            IOptions<ExternalUrlsOptions> externalUrlsOptions,
            IFacadeService facadeService)
        {
            _sessionManager = sessionManager;
            _pathBase = configuration.GetValue<string>(ConfigKeys.PathBase);
            _submissionFiltersConfig = submissionFiltersConfig.Value;
            _externalUrlsOptions = externalUrlsOptions.Value;
            _facadeService = facadeService;
        }

        [HttpGet]
        [Route(PagePath.Registrations)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Registrations(
            RegistrationFiltersModel registrationFiltersModel,
            int? pageNumber,
            bool? clearFilters,
            EndpointResponseStatus? rejectRegistrationResult = null,
            EndpointResponseStatus? acceptRegistrationResult = null,
            string? organisationName = null,
            bool? export = null)
        {
            registrationFiltersModel.SearchSubmissionYears = registrationFiltersModel.SearchSubmissionYears
                ?.Where(x => _submissionFiltersConfig.OrgYears.Contains(x))
                .ToArray();

            registrationFiltersModel.SearchSubmissionPeriods = registrationFiltersModel.SearchSubmissionPeriods
                ?.Where(x => _submissionFiltersConfig.OrgPeriods.Contains(x))
                .ToArray();

            if (export == true)
            {
                var stream = await _facadeService.GetRegistrationSubmissionsCsv(new GetRegistrationSubmissionsCsvRequest
                {
                   SearchOrganisationName = registrationFiltersModel.SearchOrganisationName,
                   SearchOrganisationId = registrationFiltersModel.SearchOrganisationId,
                   IsDirectProducerChecked = registrationFiltersModel.IsDirectProducerChecked,
                   IsComplianceSchemeChecked = registrationFiltersModel.IsComplianceSchemeChecked,
                   IsPendingRegistrationChecked = registrationFiltersModel.IsPendingRegistrationChecked,
                   IsAcceptedRegistrationChecked = registrationFiltersModel.IsAcceptedRegistrationChecked,
                   IsRejectedRegistrationChecked = registrationFiltersModel.IsRejectedRegistrationChecked,
                   SearchSubmissionPeriods = registrationFiltersModel.SearchSubmissionPeriods,
                   SearchSubmissionYears = registrationFiltersModel.SearchSubmissionYears
                });

                return File(stream, "text/csv", "registration-submissions.csv");
            }

            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            session ??= new JourneySession();
            session.RegulatorRegistrationSession.RegistrationFiltersModel ??= new RegistrationFiltersModel();

            if (clearFilters.HasValue && clearFilters.Value)
            {
                ClearFilters(session);
            }

            SetCustomBackLink();

            SetOrResetFilterValuesInSession(session, registrationFiltersModel);

            var model = new RegistrationsViewModel
            {
                RegistrationFilters = new RegistrationFiltersModel()
                {
                    SearchOrganisationName = session.RegulatorRegistrationSession.RegistrationFiltersModel.SearchOrganisationName,
                    SearchOrganisationId = session.RegulatorRegistrationSession.RegistrationFiltersModel.SearchOrganisationId,
                    IsDirectProducerChecked = session.RegulatorRegistrationSession.RegistrationFiltersModel.IsDirectProducerChecked,
                    IsComplianceSchemeChecked = session.RegulatorRegistrationSession.RegistrationFiltersModel.IsComplianceSchemeChecked,
                    IsPendingRegistrationChecked = session.RegulatorRegistrationSession.RegistrationFiltersModel.IsPendingRegistrationChecked,
                    IsAcceptedRegistrationChecked = session.RegulatorRegistrationSession.RegistrationFiltersModel.IsAcceptedRegistrationChecked,
                    IsRejectedRegistrationChecked = session.RegulatorRegistrationSession.RegistrationFiltersModel.IsRejectedRegistrationChecked,
                    SearchSubmissionYears = session.RegulatorRegistrationSession.RegistrationFiltersModel.SearchSubmissionYears,
                    SearchSubmissionPeriods = session.RegulatorRegistrationSession.RegistrationFiltersModel.SearchSubmissionPeriods
                },
                PageNumber = pageNumber ?? 1,
                PowerBiLogin = _externalUrlsOptions.PowerBiLogin,
                RejectRegistrationResult = rejectRegistrationResult,
                AcceptRegistrationResult = acceptRegistrationResult,
                OrganisationName = organisationName,
                SubmissionYears = _submissionFiltersConfig.OrgYears,
                SubmissionPeriods = _submissionFiltersConfig.OrgPeriods,
                NationId = session.UserData.Organisations?.FirstOrDefault()?.NationId ?? 0
            };

            await SaveSessionAndJourney(session, PagePath.Registrations, PagePath.Registrations);

            return View(model);
        }

        [HttpPost]
        [Route(PagePath.Registrations)]
        public async Task<IActionResult> Registrations(string jsonRegistration)
        {
            var registrationSubmission = JsonSerializer.Deserialize<Registration>(jsonRegistration);
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            session ??= new JourneySession();
            session.RegulatorRegistrationSession.OrganisationRegistration = registrationSubmission;

            await SaveSession(session);
            return RedirectToAction("RegistrationDetails", "Registrations");
        }

        [HttpGet]
        [Route(PagePath.RegistrationDetails)]
        public async Task<IActionResult> RegistrationDetails()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            var registration = session.RegulatorRegistrationSession.OrganisationRegistration;

            var model = new RegistrationDetailsViewModel
            {
                OrganisationName = registration.OrganisationName,
                BuildingName = registration.BuildingName,
                SubBuildingName = registration.SubBuildingName,
                BuildingNumber = registration.BuildingNumber,
                Street = registration.Street,
                Locality = registration.Locality,
                DependantLocality = registration.DependantLocality,
                Town = registration.Town,
                County = registration.County,
                Country = registration.Country,
                PostCode = registration.PostCode,
                OrganisationType = registration.OrganisationType.GetDescription(),
                OrganisationReferenceNumber = registration.OrganisationReference,
                FormattedTimeAndDateOfSubmission = DateTimeHelpers.FormatTimeAndDateForSubmission(registration.RegistrationDate),
                SubmissionId = registration.SubmissionId,
                SubmissionPeriod = registration.SubmissionPeriod,
                SubmittedBy = $"{registration.FirstName} {registration.LastName}",
                AccountRole = registration.ServiceRole,
                Telephone = registration.Telephone,
                Email = registration.Email,
                Status = registration.Decision,
                IsResubmission = registration.IsResubmission,
                RejectionReason = registration.RejectionComments,
                PreviousRejectionReason = registration.PreviousRejectionComments,
                PowerBiLogin = _externalUrlsOptions.PowerBiLogin,
                CompaniesHouseNumber = registration.CompaniesHouseNumber,
                OrganisationDetailsFileId = registration.OrganisationDetailsFileId,
                OrganisationDetailsFileName = registration.OrganisationDetailsFileName,
                PartnershipDetailsFileId = registration.PartnershipFileId,
                PartnershipDetailsFileName = registration.PartnershipFileName,
                BrandDetailsFileId = registration.BrandsFileId,
                BrandDetailsFileName = registration.BrandsFileName,
                DeclaredBy = registration.OrganisationType == OrganisationType.ComplianceScheme
                    ? DeclaredByComplianceScheme
                    : $"{registration.FirstName} {registration.LastName}"
            };

            await SaveSessionAndJourney(session, PagePath.Registrations, PagePath.RegistrationDetails);
            SetBackLink(session, PagePath.RegistrationDetails);

            return View(nameof(RegistrationDetails), model);
        }

        [HttpGet]
        [Route(PagePath.RejectRegistration)]
        public async Task<IActionResult> RejectRegistration()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            var model = new RejectRegistrationViewModel();

            await SaveSessionAndJourney(session, PagePath.RegistrationDetails, PagePath.RejectRegistration);
            SetBackLink(session, PagePath.RejectRegistration);

            return View(nameof(RejectRegistration), model);
        }

        [HttpPost]
        [Route(PagePath.RejectRegistration)]
        public async Task<IActionResult> RejectRegistration(RejectRegistrationViewModel model)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

            if (!ModelState.IsValid)
            {
                SetBackLink(session, PagePath.RejectRegistration);
                return View(nameof(RejectRegistration), model);
            }

            var request = new RegulatorRegistrationDecisionCreateRequest
            {
                SubmissionId = session.RegulatorRegistrationSession.OrganisationRegistration.SubmissionId,
                Decision = RegulatorDecision.Rejected,
                Comments = model.ReasonForRejection,
                FileId = session.RegulatorRegistrationSession.OrganisationRegistration.OrganisationDetailsFileId,
                OrganisationId = session.RegulatorRegistrationSession.OrganisationRegistration.OrganisationId,
                OrganisationName = session.RegulatorRegistrationSession.OrganisationRegistration.OrganisationName,
                OrganisationNumber = session.RegulatorRegistrationSession.OrganisationRegistration.OrganisationReference,
                SubmissionPeriod = session.RegulatorRegistrationSession.OrganisationRegistration.SubmissionPeriod

            };

            var result = await _facadeService.SubmitRegistrationDecision(request);

            var organisationName = session.RegulatorRegistrationSession.OrganisationRegistration.OrganisationName;
            session.RegulatorRegistrationSession.OrganisationRegistration = null;

            return await SaveSessionAndRedirect(
                session,
                nameof(Registrations),
                PagePath.RejectRegistration,
                PagePath.Registrations,
                new
                {
                    rejectRegistrationResult = result,
                    organisationName = organisationName
                });
        }

        [HttpGet]
        [Route(PagePath.AcceptRegistrationSubmission)]
        public async Task<IActionResult> AcceptRegistrationSubmission()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            var model = new AcceptRegistrationSubmissionViewModel();


            await SaveSessionAndJourney(session, PagePath.RegistrationDetails, PagePath.AcceptRegistrationSubmission);
            SetBackLink(session, PagePath.AcceptRegistrationSubmission);

            return View(nameof(AcceptRegistrationSubmission), model);
        }

        [HttpPost]
        [Route(PagePath.AcceptRegistrationSubmission)]
        public async Task<IActionResult> AcceptRegistrationSubmission(AcceptRegistrationSubmissionViewModel model)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

            if (!ModelState.IsValid)
            {
                SetBackLink(session, PagePath.AcceptRegistrationSubmission);
                return View(nameof(AcceptRegistrationSubmission), model);
            }

            if (model.Accepted != true)
            {
                return await SaveSessionAndRedirect(
                    session,
                    nameof(RegistrationDetails),
                    PagePath.AcceptRegistrationSubmission,
                    PagePath.RegistrationDetails,
                    new { });
            }

            var request = new RegulatorRegistrationDecisionCreateRequest
            {
                SubmissionId = session.RegulatorRegistrationSession.OrganisationRegistration.SubmissionId,
                Decision = RegulatorDecision.Accepted,
                FileId = session.RegulatorRegistrationSession.OrganisationRegistration.OrganisationDetailsFileId,
                OrganisationId = session.RegulatorRegistrationSession.OrganisationRegistration.OrganisationId,
                OrganisationName = session.RegulatorRegistrationSession.OrganisationRegistration.OrganisationName,
                OrganisationNumber = session.RegulatorRegistrationSession.OrganisationRegistration.OrganisationReference,
                SubmissionPeriod = session.RegulatorRegistrationSession.OrganisationRegistration.SubmissionPeriod
                

            };
            var result = await _facadeService.SubmitRegistrationDecision(request);

            var organisationName = session.RegulatorRegistrationSession.OrganisationRegistration.OrganisationName;
            session.RegulatorRegistrationSession.OrganisationRegistration = null;

            return await SaveSessionAndRedirect(
                session,
                nameof(Registrations),
                PagePath.AcceptRegistrationSubmission,
                PagePath.Registrations,
                new
                {
                    acceptRegistrationResult = result,
                    organisationName = organisationName
                });

        }

        [HttpGet]
        [Route(PagePath.OrganisationDetailsFileDownload)]
        public IActionResult OrganisationDetailsFileDownload()
        {
            return View("OrganisationDetailsFileDownload");
        }

        [HttpGet]
        [Route(PagePath.OrganisationDetailsFileDownloadFailed)]
        public IActionResult OrganisationDetailsFileDownloadFailed()
        {
            var model = new OrganisationDetailsFileDownloadViewModel(true, false);
            return View("OrganisationDetailsFileDownloadFailed", model);
        }

        [HttpGet]
        [Route(PagePath.OrganisationDetailsFileDownloadSecurityWarning)]
        public IActionResult OrganisationDetailsFileDownloadSecurityWarning()
        {
            var model = new OrganisationDetailsFileDownloadViewModel(true, true);
            return View("OrganisationDetailsFileDownloadFailed", model);
        }

        [HttpGet]
        [Route(PagePath.FileDownload)]
        public async Task<IActionResult> FileDownload(string downloadType)
        {
            TempData["DownloadCompleted"] = false;
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            session.RegulatorRegistrationSession.FileDownloadRequestType = downloadType;
            await SaveSession(session);

            return RedirectToAction(nameof(OrganisationDetailsFileDownload), "Registrations");
        }

        [HttpGet]
        [Route("[controller]/" + nameof(FileDownloadInProgress))]
        public async Task<IActionResult> FileDownloadInProgress()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            var registration = session.RegulatorRegistrationSession.OrganisationRegistration;
            var fileDownloadModel = CreateFileDownloadRequest(session, registration);

            if (fileDownloadModel == null)
            {
                return RedirectToAction(nameof(OrganisationDetailsFileDownloadFailed));
            }

            var response = await _facadeService.GetFileDownload(fileDownloadModel);

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return RedirectToAction(nameof(OrganisationDetailsFileDownloadSecurityWarning));
            }
            else if (response.IsSuccessStatusCode)
            {
                var fileStream = await response.Content.ReadAsStreamAsync();
                var contentDisposition = response.Content.Headers.ContentDisposition;
                var fileName = contentDisposition?.FileNameStar ?? contentDisposition?.FileName ?? registration.OrganisationDetailsFileName;
                TempData["DownloadCompleted"] = true;

                return File(fileStream, "application/octet-stream", fileName);
            }
            else
            {
                return RedirectToAction(nameof(OrganisationDetailsFileDownloadFailed));
            }
        }

        private static FileDownloadRequest CreateFileDownloadRequest(JourneySession session, Registration registration)
        {
            var fileDownloadModel = new FileDownloadRequest
            {
                SubmissionId = registration.SubmissionId,
                SubmissionType = SubmissionType.Registration
            };

            switch (session.RegulatorRegistrationSession.FileDownloadRequestType)
            {
                case FileDownloadTypes.OrganisationDetails:
                    fileDownloadModel.FileId = registration.OrganisationDetailsFileId;
                    fileDownloadModel.BlobName = registration.CompanyDetailsBlobName;
                    fileDownloadModel.FileName = registration.OrganisationDetailsFileName;
                    break;
                case FileDownloadTypes.BrandDetails:
                    fileDownloadModel.FileId = registration.BrandsFileId;
                    fileDownloadModel.BlobName = registration.BrandsBlobName;
                    fileDownloadModel.FileName = registration.BrandsFileName;
                    break;
                case FileDownloadTypes.PartnershipDetails:
                    fileDownloadModel.FileId = registration.PartnershipFileId;
                    fileDownloadModel.BlobName = registration.PartnershipBlobName;
                    fileDownloadModel.FileName = registration.PartnershipFileName;
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

        private void SetBackLink(JourneySession session, string currentPagePath) =>
            ViewBag.BackLinkToDisplay =
                session.RegulatorRegistrationSession.Journey.PreviousOrDefault(currentPagePath) ?? string.Empty;

        private static void SetOrResetFilterValuesInSession(JourneySession session, RegistrationFiltersModel registrationFiltersModel)
        {
            var regulatorRegistrationSession = session.RegulatorRegistrationSession;
            if (registrationFiltersModel.ClearFilters)
            {
                ClearFilters(session);
            }
            else
            {
                if (IsFilterable(registrationFiltersModel))
                {
                    regulatorRegistrationSession.RegistrationFiltersModel.SearchOrganisationName = registrationFiltersModel.SearchOrganisationName;
                    regulatorRegistrationSession.RegistrationFiltersModel.SearchOrganisationId = registrationFiltersModel.SearchOrganisationId;
                    regulatorRegistrationSession.RegistrationFiltersModel.IsDirectProducerChecked = registrationFiltersModel.IsDirectProducerChecked;
                    regulatorRegistrationSession.RegistrationFiltersModel.IsComplianceSchemeChecked = registrationFiltersModel.IsComplianceSchemeChecked;
                    regulatorRegistrationSession.RegistrationFiltersModel.IsPendingRegistrationChecked = registrationFiltersModel.IsPendingRegistrationChecked;
                    regulatorRegistrationSession.RegistrationFiltersModel.IsAcceptedRegistrationChecked = registrationFiltersModel.IsAcceptedRegistrationChecked;
                    regulatorRegistrationSession.RegistrationFiltersModel.IsRejectedRegistrationChecked = registrationFiltersModel.IsRejectedRegistrationChecked;
                    regulatorRegistrationSession.RegistrationFiltersModel.SearchSubmissionYears = registrationFiltersModel.SearchSubmissionYears;
                    regulatorRegistrationSession.RegistrationFiltersModel.SearchSubmissionPeriods = registrationFiltersModel.SearchSubmissionPeriods;
                    regulatorRegistrationSession.PageNumber = 1;
                }
            }
        }

        private static void ClearFilters(JourneySession session)
        {
            session.RegulatorRegistrationSession.PageNumber = 1;
            session.RegulatorRegistrationSession.RegistrationFiltersModel.SearchOrganisationName = string.Empty;
            session.RegulatorRegistrationSession.RegistrationFiltersModel.SearchOrganisationId = string.Empty;
            session.RegulatorRegistrationSession.RegistrationFiltersModel.IsDirectProducerChecked = false;
            session.RegulatorRegistrationSession.RegistrationFiltersModel.IsComplianceSchemeChecked = false;
            session.RegulatorRegistrationSession.RegistrationFiltersModel.IsPendingRegistrationChecked = false;
            session.RegulatorRegistrationSession.RegistrationFiltersModel.IsAcceptedRegistrationChecked = false;
            session.RegulatorRegistrationSession.RegistrationFiltersModel.IsRejectedRegistrationChecked = false;
            session.RegulatorRegistrationSession.RegistrationFiltersModel.SearchSubmissionYears = Array.Empty<int>();
            session.RegulatorRegistrationSession.RegistrationFiltersModel.SearchSubmissionPeriods = Array.Empty<string>();
        }

        private static bool IsFilterable(RegistrationFiltersModel registrationFiltersModel) =>
        (
            (!string.IsNullOrEmpty(registrationFiltersModel.SearchOrganisationName) ||
             registrationFiltersModel.IsDirectProducerChecked ||
             registrationFiltersModel.IsComplianceSchemeChecked ||
             registrationFiltersModel.IsPendingRegistrationChecked ||
             registrationFiltersModel.IsAcceptedRegistrationChecked ||
             registrationFiltersModel.IsRejectedRegistrationChecked)
            || registrationFiltersModel.SearchSubmissionYears?.Length > 0
            || registrationFiltersModel.SearchSubmissionPeriods?.Length > 0
            || registrationFiltersModel.IsFilteredSearch);

        private async Task SaveSessionAndJourney(JourneySession session, string currentPagePath, string? nextPagePath)
        {
            ClearRestOfJourney(session, currentPagePath);

            session.RegulatorRegistrationSession.Journey.AddIfNotExists(nextPagePath);

            await SaveSession(session);
        }

        private static void ClearRestOfJourney(JourneySession session, string currentPagePath)
        {
            var index = session.RegulatorRegistrationSession.Journey.IndexOf(currentPagePath);

            // this also cover if current page not found (index = -1) then it clears all pages
            session.RegulatorRegistrationSession.Journey = session.RegulatorRegistrationSession.Journey.Take(index + 1).ToList();
        }

        private async Task SaveSession(JourneySession session) =>
            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

        private void SetCustomBackLink()
        {
            string pathBase = _pathBase.TrimStart('/').TrimEnd('/');
            ViewBag.CustomBackLinkToDisplay = $"/{pathBase}/{PagePath.Home}";
        }

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
    }
}