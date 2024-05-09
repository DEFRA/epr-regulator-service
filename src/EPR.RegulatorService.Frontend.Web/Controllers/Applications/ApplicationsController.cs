using EPR.Common.Authorization.Constants;
using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.Applications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Resources;

namespace EPR.RegulatorService.Frontend.Web.Controllers.Applications
{
    [Authorize(Policy = PolicyConstants.RegulatorBasicPolicy)]
    public class ApplicationsController : RegulatorSessionBaseController
    {
        private const string CommentsFieldRequiredErrorMessage = "The Comments field is required.";
        private readonly IFacadeService _facadeService;
        private readonly ISessionManager<JourneySession> _sessionManager;
        private readonly TransferOrganisationConfig _transferOrganisationConfig;

        public ApplicationsController(
            ISessionManager<JourneySession> sessionManager,
            IFacadeService facadeService,
            IOptions<TransferOrganisationConfig> transferOrganisationOptions,
            IConfiguration configuration,
            ILogger<ApplicationsController> logger)
        : base(sessionManager, logger, configuration)
        {
            _sessionManager = sessionManager;
            _facadeService = facadeService;
            _transferOrganisationConfig = transferOrganisationOptions.Value;
        }

        [HttpGet]
        [Consumes("application/json")]
        [Route(PagePath.Applications)]
        [Route("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Applications(int? pageNumber = null)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new();
            session.RegulatorSession.OrganisationId = null;
            session.RegulatorSession.OrganisationName = string.Empty;
            session.RegulatorSession.RegulatorOrganisations = new List<RegulatorOrganisation>();

            if (pageNumber == null)
            {
                session.RegulatorSession.CurrentPageNumber ??= 1;
            }
            else
            {
                session.RegulatorSession.CurrentPageNumber = pageNumber;
            }

            ViewBag.CustomBackLinkToDisplay = GetHomeBackLink();

            EndpointResponseStatus? transferNationResult = TempData.TryGetValue("TransferNationResult", out object? result) ? (EndpointResponseStatus)result : EndpointResponseStatus.NotSet;
            string? transferredOrganisationName = TempData.TryGetValue("TransferredOrganisationName", out object? organisationName) ? organisationName.ToString() : string.Empty;
            string? transferredOrganisationAgency = TempData.TryGetValue("TransferredOrganisationAgency", out object? organisationAgency) ? organisationAgency.ToString() : string.Empty;

            var model = new ApplicationsViewModel
            {
                PageNumber = session.RegulatorSession.CurrentPageNumber ?? 1,
                SearchOrganisationName = session.RegulatorSession.SearchOrganisationName,
                IsApprovedUserTypeChecked = session.RegulatorSession.IsApprovedUserTypeChecked,
                IsDelegatedUserTypeChecked = session.RegulatorSession.IsDelegatedUserTypeChecked,
                TransferNationResult = transferNationResult.Value,
                TransferredOrganisationName = transferredOrganisationName,
                TransferredOrganisationAgency = transferredOrganisationAgency,
                RejectedUserName = TempData.TryGetValue("RejectUserName", out object? rejectUserName) ? rejectUserName.ToString() : string.Empty,
                RejectionStatus = TempData.TryGetValue("RejectResult", out object? rejectResult) ? (EndpointResponseStatus)rejectResult : EndpointResponseStatus.NotSet,
                RejectedServiceRole = TempData.TryGetValue("RejectedRole", out object? rejectedRole) ? rejectedRole.ToString() : string.Empty
            };

            await SaveSessionAndJourney(session, PagePath.Applications, PagePath.Applications);

            return View(model);
        }

        [HttpPost]
        [Route(PagePath.Applications)]
        public async Task<IActionResult> Applications(ApplicationsRequestViewModel viewModel, string? filterType = null, Guid? organisationId = null)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new();

            // if not filtering
            if (filterType == null)
            {
                if (organisationId == null)
                {
                    _logger.LogError($"Organisation id was null.");
                    return RedirectToAction(PagePath.Error, "Error");
                }

                session.RegulatorSession.OrganisationId = organisationId;

                return await SaveSessionAndRedirect(
                    session,
                    nameof(EnrolmentRequests),
                    PagePath.Applications,
                    PagePath.EnrolmentRequests,
                    null);
            }

            //if filtering
            if (filterType == FilterActions.ClearFilters)
            {
                viewModel.ClearFilters = true;
            }

            SetFilterValues(session,
                viewModel.SearchOrganisationName,
                viewModel.IsApprovedUserTypeChecked,
                viewModel.IsDelegatedUserTypeChecked,
                viewModel.ClearFilters,
                viewModel.IsFilteredSearch);

            return await SaveSessionAndRedirect(
                session,
                nameof(Applications),
                PagePath.Applications,
                PagePath.Applications,
                null);
        }

        [HttpGet]
        [Route(PagePath.EnrolmentRequests)]
        public async Task<IActionResult> EnrolmentRequests()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            var organisationId = session.RegulatorSession.OrganisationId;
            var organisationDetails = await _facadeService.GetOrganisationEnrolments(organisationId.Value);

            session.RegulatorSession.OrganisationName = organisationDetails.OrganisationName;
            session.RegulatorSession.ReferenceNumber = organisationDetails.OrganisationReferenceNumber;
            session.RegulatorSession.RegulatorNation = organisationDetails.NationId;

            var model = GetEnrolmentRequestsViewModel(organisationDetails);
            model.AcceptStatus = TempData.TryGetValue("AcceptResult", out object? acceptResult) ? (EndpointResponseStatus)acceptResult : EndpointResponseStatus.NotSet;
            model.AcceptedFirstName = TempData.TryGetValue("AcceptFirstName", out object? acceptFirstName) ? acceptFirstName.ToString() : string.Empty;
            model.AcceptedLastName = TempData.TryGetValue("AcceptLastName", out object? acceptLastName) ? acceptLastName.ToString() : string.Empty;
            model.AcceptedRole = TempData.TryGetValue("AcceptedRole", out object? acceptedRole) ? acceptedRole.ToString() : string.Empty;
            model.RejectionStatus = TempData.TryGetValue("RejectResult", out object? rejectResult) ? (EndpointResponseStatus)rejectResult : EndpointResponseStatus.NotSet;
            model.RejectedUserName = TempData.TryGetValue("RejectUserName", out object? rejectUserName) ? rejectUserName.ToString() : string.Empty;
            model.RejectedServiceRole = TempData.TryGetValue("RejectedRole", out object? rejectedRole) ? rejectedRole.ToString() : string.Empty;

            await SaveSessionAndJourney(session, PagePath.Applications, PagePath.EnrolmentRequests);
            SetBackLink(session, PagePath.EnrolmentRequests);

            return View(nameof(EnrolmentRequests), model);
        }

        [HttpPost]
        [Route(PagePath.EnrolmentRequests)]
        public async Task<IActionResult> EnrolmentRequests(UserEnrolment userEnrolment, string journeyType)
        {
            var session = (await _sessionManager.GetSessionAsync(HttpContext.Session)) ?? new JourneySession();

            if (journeyType == JourneyType.Accept)
            {
               return await AcceptApplication(userEnrolment.User.FirstName, userEnrolment.User.LastName, userEnrolment.User.Email,
                    userEnrolment.User.Enrolment.ServiceRole);
            }

            session.RegulatorSession.RejectUserJourneyData = new()
            {
                FirstName = userEnrolment.User.FirstName,
                LastName = userEnrolment.User.LastName,
                Email = userEnrolment.User.Email,
                ServiceRole = userEnrolment.User.Enrolment.ServiceRole,
                Decision = RegulatorDecision.Rejected
            };

            await SaveSession(session);
            return RedirectToAction("EnrolmentDecision", "Applications");
        }

        [HttpGet]
        [Route(PagePath.TransferApplication)]
        public async Task<IActionResult> TransferApplication()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

            var regulatorOrganisations = _transferOrganisationConfig.Data.Organisations;
            session.RegulatorSession.RegulatorOrganisations = regulatorOrganisations.ToList();

            var model = new TransferApplicationViewModel
            {
                UserNation = session.RegulatorSession.RegulatorNation,
                OrganisationName = session.RegulatorSession.OrganisationName,
                RegulatorOrganisations = session.RegulatorSession.RegulatorOrganisations
            };

            await SaveSessionAndJourney(session, PagePath.EnrolmentRequests, PagePath.TransferApplication);
            SetBackLink(session, PagePath.TransferApplication);

            return View(nameof(TransferApplication), model);
        }

        [HttpPost]
        [Route(PagePath.TransferApplication)]
        public async Task<IActionResult> TransferApplication(TransferApplicationViewModel model)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new();

            if (!ModelState.IsValid)
            {
                SetBackLink(session, PagePath.TransferApplication);
                model.RegulatorOrganisations = session.RegulatorSession.RegulatorOrganisations;
                return View(nameof(TransferApplication), model);
            }

            var transferNote = model.TransferNotes.FirstOrDefault(x => x.AgencyIndex == model.AgencyIndex) ?? new();

            OrganisationTransferNationRequest organisationNationTransfer = new OrganisationTransferNationRequest()
            {
                OrganisationId = (session.RegulatorSession.OrganisationId == null) ? Guid.Empty : session.RegulatorSession.OrganisationId.Value,
                TransferNationId = transferNote.NationId,
                TransferComments = transferNote.Notes ?? string.Empty
            };

            var result = await _facadeService.TransferOrganisationNation(organisationNationTransfer);

            TempData["TransferNationResult"] = result;
            TempData["TransferredOrganisationName"] = session.RegulatorSession.OrganisationName;
            TempData["TransferredOrganisationAgency"] = transferNote.AgencyName;

            return await SaveSessionAndRedirect(
                session,
                nameof(Applications),
                PagePath.TransferApplication,
                PagePath.Applications,
                null);
        }

        [HttpGet]
        [Route(PagePath.EnrolmentDecision)]
        public async Task<IActionResult> EnrolmentDecision()
        {
            var session = (await _sessionManager.GetSessionAsync(HttpContext.Session)) ?? new JourneySession();

            var rejectedJourneyData = session.RegulatorSession.RejectUserJourneyData;

            var organisationId = session.RegulatorSession.OrganisationId.Value;

            if (rejectedJourneyData.ServiceRole == ServiceRole.ApprovedPerson)
            {
                rejectedJourneyData.ApprovedUserFirstName = rejectedJourneyData.FirstName;
                rejectedJourneyData.ApprovedUserLastName = rejectedJourneyData.LastName;
            }
            else
            {
                var organisationDetails = await _facadeService.GetOrganisationEnrolments(organisationId);

                var approvedUser = organisationDetails.Users.FirstOrDefault(x => x.Enrolment.ServiceRole == ServiceRole.ApprovedPerson) ?? new();
                rejectedJourneyData.ApprovedUserFirstName = approvedUser.FirstName;
                rejectedJourneyData.ApprovedUserLastName = approvedUser.LastName;
            }

            await SaveSessionAndJourney(session, PagePath.EnrolmentRequests, PagePath.EnrolmentDecision);

            SetBackLink(session, PagePath.EnrolmentDecision);

            var model = new EnrolmentDecisionViewModel()
            {
                OrganisationId = organisationId,
                RejectedUserFirstName = rejectedJourneyData.FirstName,
                RejectedUserLastName = rejectedJourneyData.LastName,
                ApprovedUserFirstName = rejectedJourneyData.ApprovedUserFirstName,
                ApprovedUserLastName = rejectedJourneyData.ApprovedUserLastName,
            };

            return View(nameof(EnrolmentDecision), model);
        }

        [HttpPost]
        [Route(PagePath.EnrolmentDecision)]
        public async Task<IActionResult> EnrolmentDecision(EnrolmentDecisionViewModel model)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

            var rejectedUserData = session.RegulatorSession.RejectUserJourneyData;

            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    var requiredError = state.Errors.FirstOrDefault(x => x.ErrorMessage == CommentsFieldRequiredErrorMessage);

                    if (requiredError != null)
                    {
                        state.Errors.Remove(requiredError);

                        ResourceManager rm = new ResourceManager("EPR.RegulatorService.Frontend.Web.Resources.Views.Applications.EnrolmentDecision", Assembly.GetExecutingAssembly());
                        string message = rm.GetString("Reject.CommentsRequiredError");
                        state.Errors.Add(new ModelError(string.Format(message, model.RejectedUserFirstName + " " + model.RejectedUserLastName)));
                    }
                }
                SetBackLink(session, PagePath.EnrolmentDecision);
                return View(nameof(EnrolmentDecision), model);
            }

            await SaveSessionAndJourney(session, PagePath.Applications, PagePath.EnrolmentRequests);

            SetBackLink(session, PagePath.EnrolmentDecision);

            var organisationDetails = await _facadeService.GetOrganisationEnrolments(session.RegulatorSession.OrganisationId.Value);
            var approvedUser = organisationDetails.Users.FirstOrDefault(x => x.Enrolment.ServiceRole == ServiceRole.ApprovedPerson) ?? new();

            var delegatedUsers = organisationDetails.Users.Where(x => x.Enrolment.ServiceRole == ServiceRole.DelegatedPerson);

            var approvedUserEmailDetails = new EmailDetails();
            var delegateUsersEmailDetails = new List<EmailDetails>();

            approvedUserEmailDetails.UserFirstName = model.ApprovedUserFirstName;
            approvedUserEmailDetails.UserSurname = model.ApprovedUserLastName;
            approvedUserEmailDetails.Email = approvedUser.Email ?? string.Empty;

            if (rejectedUserData.ServiceRole == ServiceRole.ApprovedPerson)
            {
                delegateUsersEmailDetails.AddRange(delegatedUsers.Select(delegatedUser => new EmailDetails
                {
                    UserFirstName = delegatedUser.FirstName,
                    UserSurname = delegatedUser.LastName,
                    Email = delegatedUser.Email
                }));
            }
            else
            {
                delegateUsersEmailDetails.Add(new EmailDetails()
                {
                    UserFirstName = model.RejectedUserFirstName,
                    UserSurname = model.RejectedUserLastName,
                    Email = rejectedUserData.Email
                });
            }

            var updateEnrolment = new UpdateEnrolment()
            {
                EnrolmentId =
                    GetEnrolmentId(rejectedUserData.ServiceRole, approvedUser, delegatedUsers, rejectedUserData.Email),
                EnrolmentStatus = rejectedUserData.Decision,
                Comments = model.Comments
            };

            var result = await _facadeService.UpdateEnrolment(updateEnrolment);

            if (result == EndpointResponseStatus.Success)
            {
                rejectedUserData.ResponseStatus = EndpointResponseStatus.Success;

                var request = new EnrolmentDecisionRequest
                {
                    OrganisationNumber = organisationDetails.OrganisationReferenceNumber,
                    OrganisationName = organisationDetails.OrganisationName,
                    ApprovedUser = approvedUserEmailDetails,
                    DelegatedUsers = delegateUsersEmailDetails,
                    RejectionComments = model.Comments,
                    RegulatorRole = rejectedUserData.ServiceRole,
                    Decision = rejectedUserData.Decision
                };

                await _facadeService.SendEnrolmentEmails(request);
            }
            else
            {
                rejectedUserData.ResponseStatus = EndpointResponseStatus.Fail;
            }

            TempData["RejectResult"] = rejectedUserData.ResponseStatus;
            TempData["RejectUserName"] = $"{rejectedUserData.FirstName} {rejectedUserData.LastName}";
            TempData["RejectedRole"] = rejectedUserData.ServiceRole;

            if (rejectedUserData.ServiceRole == ServiceRole.ApprovedPerson)
            {
                return await SaveSessionAndRedirect(
                    session, nameof(Applications), PagePath.EnrolmentDecision, PagePath.Applications, null);
            }

            return await SaveSessionAndRedirect(
                session, nameof(EnrolmentRequests), PagePath.Applications, PagePath.EnrolmentRequests, null);
        }

        public async Task<IActionResult> AcceptApplication(string? acceptedUserFirstName, string? acceptedUserLastName, string? acceptedUserEmail, string? serviceRole)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new();
            await SaveSessionAndJourney(session, PagePath.Applications, PagePath.EnrolmentDecision);

            SetBackLink(session, PagePath.Applications);

            var organisationDetails =
                await _facadeService.GetOrganisationEnrolments(session.RegulatorSession.OrganisationId.Value);
            var approvedUser = organisationDetails.Users
                .FirstOrDefault(x => x.Enrolment.ServiceRole == ServiceRole.ApprovedPerson);
            var delegatedUsers = organisationDetails.Users
                .Where(x => x.Enrolment.ServiceRole == ServiceRole.DelegatedPerson);

            if (serviceRole == ServiceRole.ApprovedPerson)
            {
                delegatedUsers = new List<User>();
            }
            else if (serviceRole == ServiceRole.DelegatedPerson)
            {
                delegatedUsers = delegatedUsers.Where(x => x.Email == acceptedUserEmail);
            }

            try
            {
                var request = new EnrolmentDecisionRequest()
                {
                    OrganisationNumber = organisationDetails.OrganisationReferenceNumber,
                    OrganisationName = organisationDetails.OrganisationName,
                    ApprovedUser = new EmailDetails()
                    {
                        UserFirstName = approvedUser.FirstName,
                        UserSurname = approvedUser.LastName,
                        Email = approvedUser.Email
                    },
                    DelegatedUsers = delegatedUsers.Select(x => new EmailDetails()
                    {
                        UserFirstName = x.FirstName,
                        UserSurname = x.LastName,
                        Email = x.Email
                    }).ToList(),
                    RejectionComments = String.Empty,
                    RegulatorRole = serviceRole,
                    Decision = RegulatorDecision.Approved,
                };
                var sendEmailResult = await _facadeService.SendEnrolmentEmails(request);

                if (sendEmailResult == EndpointResponseStatus.Success)
                {
                    var updateEnrolmentRequest = new UpdateEnrolment()
                    {
                        EnrolmentId = GetEnrolmentId(serviceRole, approvedUser, delegatedUsers, acceptedUserEmail),
                        EnrolmentStatus = RegulatorDecision.Approved,
                        Comments = string.Empty
                    };

                    var result = await _facadeService.UpdateEnrolment(updateEnrolmentRequest);
                    if (result == EndpointResponseStatus.Success)
                    {
                        session.RegulatorSession.OrganisationId = organisationDetails.OrganisationId;

                        TempData["AcceptResult"] = EndpointResponseStatus.Success;
                        TempData["AcceptFirstName"] = acceptedUserFirstName;
                        TempData["AcceptLastName"] = acceptedUserLastName;
                        TempData["AcceptedRole"] = serviceRole;

                        await SaveSession(session);

                        return RedirectToAction("EnrolmentRequests", "Applications");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error accepting application for: {acceptedUserEmail} in organisation {organisationDetails.OrganisationId}");
            }

            ModelState.AddModelError("Update failed", "Update failed");
            var enrolmentRequestsViewModel = GetEnrolmentRequestsViewModel(organisationDetails);

            return View(nameof(EnrolmentRequests), enrolmentRequestsViewModel);
        }

        private EnrolmentRequestsViewModel GetEnrolmentRequestsViewModel(OrganisationEnrolments organisationDetails)
        {
            var approvedUser = organisationDetails.Users.FirstOrDefault(x => x.Enrolment.ServiceRole == ServiceRole.ApprovedPerson) ?? new();

            var delegatedUsers = organisationDetails.Users.Where(x => x.Enrolment.ServiceRole == ServiceRole.DelegatedPerson);

            bool isApprovedUserAccepted = approvedUser.Enrolment.EnrolmentStatus == EnrolmentStatus.Approved;

            var model = new EnrolmentRequestsViewModel
            {
                OrganisationId = organisationDetails.OrganisationId,
                OrganisationName = organisationDetails.OrganisationName,
                ReferenceNumber = organisationDetails.OrganisationReferenceNumber,
                OrganisationType = organisationDetails.OrganisationType,
                IsComplianceScheme = organisationDetails.IsComplianceScheme,
                BusinessAddress = new BusinessAddress
                {
                    BuildingName = organisationDetails.BusinessAddress.BuildingName,
                    BuildingNumber = organisationDetails.BusinessAddress.BuildingNumber,
                    Street = organisationDetails.BusinessAddress.Street,
                    County = organisationDetails.BusinessAddress.County,
                    PostCode = organisationDetails.BusinessAddress.PostCode
                },
                CompaniesHouseNumber = organisationDetails.CompaniesHouseNumber,
                RegisteredNation = organisationDetails.NationName,
                IsApprovedUserAccepted = isApprovedUserAccepted,
                ApprovedUser = approvedUser,
                DelegatedUsers = delegatedUsers.ToList(),
            };

            if (organisationDetails.TransferDetails != null)
            {
                model.Transfer = new TransferBannerViewModel()
                {
                    OldRegulatorName = _transferOrganisationConfig.Data.Organisations.First(x => x.NationId == organisationDetails.TransferDetails.OldNationId).KeyValue,
                    NewRegulatorName = _transferOrganisationConfig.Data.Organisations.First(x => x.NationId == organisationDetails.NationId).KeyValue,
                    TransferredDate = organisationDetails.TransferDetails.TransferredDate
                };
            }

            return model;
        }

        private static void ClearFilters(JourneySession session)
        {
            session.RegulatorSession.SearchOrganisationName = string.Empty;
            session.RegulatorSession.IsApprovedUserTypeChecked = false;
            session.RegulatorSession.IsDelegatedUserTypeChecked = false;
            session.RegulatorSession.CurrentPageNumber = 1;
        }

        private Guid GetEnrolmentId(string serviceRole, User approvedUser, IEnumerable<User> delegatedUsers,
            string email)
        {
            if (serviceRole == ServiceRole.ApprovedPerson)
            {
                return approvedUser.Enrolment.ExternalId;
            }

            return delegatedUsers.FirstOrDefault(x => x.Email.Equals(email))?.Enrolment.ExternalId ?? Guid.Empty;
        }

        private static void SetFilterValues(JourneySession session,
            string searchOrganisationName,
            bool isApprovedUserTypeChecked,
            bool isDelegatedUserTypeChecked,
            bool clearFilters,
            bool isFilteredSearch)
        {
            var regulatorSession = session.RegulatorSession;
            if (clearFilters)
            {
                ClearFilters(session);
            }
            else
            {
                if ((!string.IsNullOrEmpty(searchOrganisationName)
                     || isApprovedUserTypeChecked
                     || isDelegatedUserTypeChecked)
                     || isFilteredSearch)
                {
                    regulatorSession.SearchOrganisationName = searchOrganisationName;
                    regulatorSession.IsApprovedUserTypeChecked = isApprovedUserTypeChecked;
                    regulatorSession.IsDelegatedUserTypeChecked = isDelegatedUserTypeChecked;
                    regulatorSession.CurrentPageNumber = 1;
                }
            }
        }
    }
}