namespace EPR.RegulatorService.Frontend.Web.Controllers.RegistrationSubmissions
{
    using System.Globalization;

    using EPR.RegulatorService.Frontend.Core.Enums;
    using EPR.RegulatorService.Frontend.Core.Extensions;
    using EPR.RegulatorService.Frontend.Core.Models;
    using EPR.RegulatorService.Frontend.Core.Models.FileDownload;
    using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
    using EPR.RegulatorService.Frontend.Core.Sessions;
    using EPR.RegulatorService.Frontend.Web.Constants;
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

    using Microsoft.AspNetCore.Mvc;

    public partial class RegistrationSubmissionsController
    {
        public static void InitialiseOrContinuePaging(RegulatorRegistrationSubmissionSession session,
                                                int? pageNumber) => session.CurrentPageNumber = pageNumber ?? session.CurrentPageNumber ?? 1;

        private RegistrationSubmissionsViewModel InitialiseOrCreateViewModel(
            RegulatorRegistrationSubmissionSession session,
            int nationId)
        {
            var existingSessionFilters = session.LatestFilterChoices ?? new RegistrationSubmissionsFilterViewModel()
            {
                PageNumber = 1,
                PageSize = 20,
                NationId = nationId
            };
            existingSessionFilters.PageNumber = session.CurrentPageNumber;

            return new RegistrationSubmissionsViewModel
            {
                NationId = nationId,
                ListViewModel = new RegistrationSubmissionsListViewModel
                {
                    NationId = nationId,
                    RegistrationsFilterModel = existingSessionFilters,
                    PaginationNavigationModel = new ViewModels.Shared.PaginationNavigationModel
                    {
                        CurrentPage = session.CurrentPageNumber.Value
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

            var sessionModelWhichMustMatchSession = _currentSession.RegulatorRegistrationSubmissionSession.OrganisationDetailsChangeHistory.TryGetValue(submissionId.Value, out var value)
                                                    ? value : _currentSession.RegulatorRegistrationSubmissionSession.SelectedRegistration;

            if (sessionModelWhichMustMatchSession?.SubmissionId != submissionId.Value)
            {
                return false;
            }

            viewModel = sessionModelWhichMustMatchSession;
            return true;
        }

        private async Task<RegistrationSubmissionOrganisationDetails> FetchFromSessionOrFacadeAsync(Guid submissionId, Func<Guid, Task<RegistrationSubmissionOrganisationDetails>> facadeMethod) =>
            _currentSession.RegulatorRegistrationSubmissionSession.SelectedRegistration?.SubmissionId == submissionId
                ? _currentSession.RegulatorRegistrationSubmissionSession.SelectedRegistration
                : await facadeMethod(submissionId);

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
                existingModel.Status = Enum.Parse<RegistrationSubmissionStatus>(regulatorDecisionRequest.Status, true);
                existingModel.SubmissionDetails.Status = existingModel.Status;
                existingModel.SubmissionDetails.DecisionDate = DateTime.UtcNow;

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

        private static RegulatorDecisionRequest GetDecisionRequest(
            RegistrationSubmissionDetailsViewModel existingModel,
            RegistrationSubmissionStatus status)
        {
            var request = new RegulatorDecisionRequest
            {
                ApplicationReferenceNumber = existingModel.ReferenceNumber,
                OrganisationId = existingModel.OrganisationId,
                SubmissionId = existingModel.SubmissionId,
                // For sending emails
                OrganisationName = existingModel.OrganisationName,
                OrganisationEmail = existingModel.SubmissionDetails.Email,
                OrganisationReference = existingModel.OrganisationReference,
                AgencyName = GetRegulatorAgencyName(existingModel.NationId),
                AgencyEmail = GetRegulatorAgencyEmail(existingModel.NationId),
                IsWelsh = existingModel.NationId == 4,
                Status = status.ToString(),
                IsResubmission = existingModel.IsResubmission
            };

            if (status == RegistrationSubmissionStatus.Granted && request.IsResubmission)
            {
                request.ExistingRegRefNumber = existingModel.RegistrationReferenceNumber;
                return request;
            }

            // For generating reference
            request.CountryName = GetCountryCodeInitial(existingModel.NationId);
            request.RegistrationSubmissionType = existingModel.OrganisationType.GetRegistrationSubmissionType();
            request.TwoDigitYear = (existingModel.RegistrationYear % 100).ToString(CultureInfo.InvariantCulture);
            request.OrganisationAccountManagementId = existingModel.OrganisationReference;
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
                    var orgFile = registration.SubmissionDetails.Files.FirstOrDefault(static x => x.Type == RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileType.company);
                    if (null != orgFile)
                    {
                        fileDownloadModel.FileId = orgFile.FileId;
                        fileDownloadModel.BlobName = orgFile.BlobName;
                        fileDownloadModel.FileName = orgFile.FileName;
                    }
                    break;
                case FileDownloadTypes.BrandDetails:
                    orgFile = registration.SubmissionDetails.Files.FirstOrDefault(static x => x.Type == RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileType.brands);
                    if (null != orgFile)
                    {
                        fileDownloadModel.FileId = orgFile.FileId;
                        fileDownloadModel.BlobName = orgFile.BlobName;
                        fileDownloadModel.FileName = orgFile.FileName;
                    }
                    break;
                case FileDownloadTypes.PartnershipDetails:
                    orgFile = registration.SubmissionDetails.Files.FirstOrDefault(static x => x.Type == RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileType.partnership);
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

    }
}