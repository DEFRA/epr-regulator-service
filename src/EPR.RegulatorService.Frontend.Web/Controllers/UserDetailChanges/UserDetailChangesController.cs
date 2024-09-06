using EPR.Common.Authorization.Constants;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;

using EPR.RegulatorService.Frontend.Web.ViewModels.UserDetailChanges;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EPR.RegulatorService.Frontend.Web.Controllers.UserDetailChanges
{
    [Authorize(Policy = PolicyConstants.RegulatorBasicPolicy)]
    public class UserDetailChangesController : RegulatorSessionBaseController
    {
        private const string CommentsFieldRequiredErrorMessage = "The Comments field is required.";
        private readonly IFacadeService _facadeService;
        private readonly ISessionManager<JourneySession> _sessionManager;
 
        public UserDetailChangesController(
            ISessionManager<JourneySession> sessionManager,
            IFacadeService facadeService,
            IConfiguration configuration,
            ILogger<UserDetailChangesController> logger)
        : base(sessionManager, logger, configuration)
        {
            _sessionManager = sessionManager;
            _facadeService = facadeService;
        }

        [HttpGet]
        [Consumes("application/json")]
        [Route(PagePath.ManageUserDetailChanges)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UserDetailChanges(int? pageNumber = null)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new();
            session.RegulatorManageUserDetailChangeSession.OrganisationId = null;
            session.RegulatorManageUserDetailChangeSession.OrganisationName = string.Empty;
            session.RegulatorManageUserDetailChangeSession.RegulatorOrganisations = new List<RegulatorOrganisation>();

            if (pageNumber == null)
            {
                session.RegulatorManageUserDetailChangeSession.CurrentPageNumber ??= 1;
            }
            else
            {
                session.RegulatorManageUserDetailChangeSession.CurrentPageNumber = pageNumber;
            }

            ViewBag.CustomBackLinkToDisplay = GetHomeBackLink();

            //EndpointResponseStatus? transferNationResult = TempData.TryGetValue("TransferNationResult", out object? result) ? (EndpointResponseStatus)result : EndpointResponseStatus.NotSet;
            //string? transferredOrganisationName = TempData.TryGetValue("TransferredOrganisationName", out object? organisationName) ? organisationName.ToString() : string.Empty;
            //string? transferredOrganisationAgency = TempData.TryGetValue("TransferredOrganisationAgency", out object? organisationAgency) ? organisationAgency.ToString() : string.Empty;

            var model = new UserDetailChangesViewModel
            {
                PageNumber = session.RegulatorManageUserDetailChangeSession.CurrentPageNumber ?? 1,
                SearchOrganisationName = session.RegulatorManageUserDetailChangeSession.SearchOrganisationName,
                IsApprovedUserTypeChecked = session.RegulatorManageUserDetailChangeSession.IsApprovedUserTypeChecked,
                IsDelegatedUserTypeChecked = session.RegulatorManageUserDetailChangeSession.IsDelegatedUserTypeChecked,


                RejectedUserName = TempData.TryGetValue("RejectUserName", out object? rejectUserName) ? rejectUserName.ToString() : string.Empty,
                RejectionStatus = TempData.TryGetValue("RejectResult", out object? rejectResult) ? (EndpointResponseStatus)rejectResult : EndpointResponseStatus.NotSet,
                RejectedServiceRole = TempData.TryGetValue("RejectedRole", out object? rejectedRole) ? rejectedRole.ToString() : string.Empty
            };


            //model.AcceptStatus = TempData.TryGetValue("AcceptResult", out object? acceptResult) ? (EndpointResponseStatus)acceptResult : EndpointResponseStatus.NotSet;
            //model.AcceptedFirstName = TempData.TryGetValue("AcceptFirstName", out object? acceptFirstName) ? acceptFirstName.ToString() : string.Empty;
            //model.AcceptedLastName = TempData.TryGetValue("AcceptLastName", out object? acceptLastName) ? acceptLastName.ToString() : string.Empty;
            //model.AcceptedRole = TempData.TryGetValue("AcceptedRole", out object? acceptedRole) ? acceptedRole.ToString() : string.Empty;
            //model.RejectionStatus = TempData.TryGetValue("RejectResult", out object? rejectResult) ? (EndpointResponseStatus)rejectResult : EndpointResponseStatus.NotSet;
            //model.RejectedUserName = TempData.TryGetValue("RejectUserName", out object? rejectUserName) ? rejectUserName.ToString() : string.Empty;
            //model.RejectedServiceRole = TempData.TryGetValue("RejectedRole", out object? rejectedRole) ? rejectedRole.ToString() : string.Empty;




            await SaveSessionAndJourney(session, PagePath.ManageUserDetailChanges, PagePath.ManageUserDetailChanges);

            return View(model);
        }

        [HttpPost]
        [Route(PagePath.ManageUserDetailChanges)]
        public async Task<IActionResult> UserDetailChanges(UserDetailChangesFilterRequestViewModel viewModel, string? filterType = null, Guid? organisationId = null, Guid ? externalId = null)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new();

            // if not filtering
            if (filterType == null)
            {
                if (externalId == null)
                {
                    _logger.LogError($"Change History ExternalId was null.");
                    return RedirectToAction(PagePath.Error, "Error");
                }
                session.RegulatorManageUserDetailChangeSession.OrganisationId = organisationId;

                session.RegulatorManageUserDetailChangeSession.ChangeHistoryExternalId = externalId;

                return await SaveSessionAndRedirect(
                    session,
                    nameof(UserDetailChangesRequest),
                    PagePath.ManageUserDetailChanges,
                    PagePath.UserDetailChangesRequest,
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
                nameof(UserDetailChanges),
                PagePath.ManageUserDetailChanges,
                PagePath.ManageUserDetailChanges,
                null);
        }

        [HttpGet]
        [Route(PagePath.UserDetailChangesRequest)]
        public async Task<IActionResult> UserDetailChangesRequest()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            var organisationId = session.RegulatorManageUserDetailChangeSession.OrganisationId;
            var changeHistoryExternalId = session.RegulatorManageUserDetailChangeSession.ChangeHistoryExternalId;
            var changeHistoryModel = await _facadeService.GetUserDetailChangeRequest(organisationId.Value,changeHistoryExternalId.Value);

            session.RegulatorManageUserDetailChangeSession.OrganisationName = changeHistoryModel.OrganisationName;
            session.RegulatorManageUserDetailChangeSession.ReferenceNumber = changeHistoryModel.OrganisationReferenceNumber;

            var model = new UserDetailChangesRequestViewModel
            { 
                ChangeHistoryModel = changeHistoryModel,
            };
            await SaveSessionAndJourney(session, PagePath.ManageUserDetailChanges, PagePath.UserDetailChangesRequest);
            SetBackLink(session, PagePath.UserDetailChangesRequest);
            return View(nameof(UserDetailChangesRequest), model);
        }

        [HttpPost]
        [Route(PagePath.UserDetailChangesRequest)]
        public async Task<IActionResult> UserDetailChangesRequest(UserDetailChangesRequestViewModel viewmodel, string journeyType)
        {
            var session = (await _sessionManager.GetSessionAsync(HttpContext.Session)) ?? new JourneySession();

            if (journeyType == JourneyType.Accept)
            {
                return await AcceptUserDetailChanges(new ManageUserDetailsChangeRequest
                {
                   ChangeHistoryExternalId = viewmodel.ChangeHistoryModel.ExternalId,
                   OrganisationId = viewmodel.ChangeHistoryModel.OrganisationId,
                   HasRegulatorAccepted = true,
                });
            }

            session.RegulatorManageUserDetailChangeSession.RejectUserDetailChangeJourneyData = new()
            {
                //FirstName = updateUserDetailRequest.User.FirstName,
                //LastName = updateUserDetailRequest.User.LastName,
                //Email = updateUserDetailRequest.User.Email,
                //ServiceRole = updateUserDetailRequest.User.Enrolment.ServiceRole,
                Decision = RegulatorDecision.Rejected
            };

            await SaveSession(session);

            return RedirectToAction(PagePath.UserDetailChangesRequest, "UserDetailChanges");
        }

        //[HttpGet]
        //[Route(PagePath.EnrolmentDecision)]
        //public async Task<IActionResult> EnrolmentDecision()
        //{
        //    var session = (await _sessionManager.GetSessionAsync(HttpContext.Session)) ?? new JourneySession();

        //    var rejectedJourneyData = session.RegulatorManageUserDetailChangeSession.RejectUserJourneyData;

        //    var organisationId = session.RegulatorManageUserDetailChangeSession.OrganisationId.Value;

        //    if (rejectedJourneyData.ServiceRole == ServiceRole.ApprovedPerson)
        //    {
        //        rejectedJourneyData.ApprovedUserFirstName = rejectedJourneyData.FirstName;
        //        rejectedJourneyData.ApprovedUserLastName = rejectedJourneyData.LastName;
        //    }
        //    else
        //    {
        //        var organisationDetails = await _facadeService.GetOrganisationEnrolments(organisationId);

        //        var approvedUser = organisationDetails.Users.FirstOrDefault(x => x.Enrolment.ServiceRole == ServiceRole.ApprovedPerson) ?? new();
        //        rejectedJourneyData.ApprovedUserFirstName = approvedUser.FirstName;
        //        rejectedJourneyData.ApprovedUserLastName = approvedUser.LastName;
        //    }

        //    await SaveSessionAndJourney(session, PagePath.EnrolmentRequests, PagePath.EnrolmentDecision);

        //    SetBackLink(session, PagePath.EnrolmentDecision);

        //    var model = new EnrolmentDecisionViewModel()
        //    {
        //        OrganisationId = organisationId,
        //        RejectedUserFirstName = rejectedJourneyData.FirstName,
        //        RejectedUserLastName = rejectedJourneyData.LastName,
        //        ApprovedUserFirstName = rejectedJourneyData.ApprovedUserFirstName,
        //        ApprovedUserLastName = rejectedJourneyData.ApprovedUserLastName,
        //    };

        //    return View(nameof(EnrolmentDecision), model);
        //}

        //[HttpPost]
        //[Route(PagePath.EnrolmentDecision)]
        //public async Task<IActionResult> EnrolmentDecision(EnrolmentDecisionViewModel model)
        //{
        //    var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        //    var rejectedUserData = session.RegulatorManageUserDetailChangeSession.RejectUserJourneyData;

        //    if (!ModelState.IsValid)
        //    {
        //        foreach (var key in ModelState.Keys)
        //        {
        //            var state = ModelState[key];
        //            var requiredError = state.Errors.FirstOrDefault(x => x.ErrorMessage == CommentsFieldRequiredErrorMessage);

        //            if (requiredError != null)
        //            {
        //                state.Errors.Remove(requiredError);

        //                ResourceManager rm = new ResourceManager("EPR.RegulatorService.Frontend.Web.Resources.Views.Applications.EnrolmentDecision", Assembly.GetExecutingAssembly());
        //                string message = rm.GetString("Reject.CommentsRequiredError", System.Globalization.CultureInfo.InvariantCulture);
        //                state.Errors.Add(new ModelError(string.Format(System.Globalization.CultureInfo.InvariantCulture, message, model.RejectedUserFirstName + " " + model.RejectedUserLastName)));
        //            }
        //        }
        //        SetBackLink(session, PagePath.EnrolmentDecision);
        //        return View(nameof(EnrolmentDecision), model);
        //    }

        //    await SaveSessionAndJourney(session, PagePath.Applications, PagePath.EnrolmentRequests);

        //    SetBackLink(session, PagePath.EnrolmentDecision);

        //    var organisationDetails = await _facadeService.GetOrganisationEnrolments(session.RegulatorManageUserDetailChangeSession.OrganisationId.Value);

        //    var approvedUser = organisationDetails.Users.FirstOrDefault(x => x.Enrolment.ServiceRole == ServiceRole.ApprovedPerson) ?? new();

        //    var delegatedUsers = organisationDetails.Users.Where(x => x.Enrolment.ServiceRole == ServiceRole.DelegatedPerson);

        //    var approvedUserEmailDetails = new EmailDetails();

        //    var delegateUsersEmailDetails = new List<EmailDetails>();

        //    approvedUserEmailDetails.UserFirstName = model.ApprovedUserFirstName;

        //    approvedUserEmailDetails.UserSurname = model.ApprovedUserLastName;

        //    approvedUserEmailDetails.Email = approvedUser.Email ?? string.Empty;

        //    if (rejectedUserData.ServiceRole == ServiceRole.ApprovedPerson)
        //    {
        //        delegateUsersEmailDetails.AddRange(delegatedUsers.Select(delegatedUser => new EmailDetails
        //        {
        //            UserFirstName = delegatedUser.FirstName,
        //            UserSurname = delegatedUser.LastName,
        //            Email = delegatedUser.Email
        //        }));
        //    }
        //    else
        //    {
        //        delegateUsersEmailDetails.Add(new EmailDetails()
        //        {
        //            UserFirstName = model.RejectedUserFirstName,
        //            UserSurname = model.RejectedUserLastName,
        //            Email = rejectedUserData.Email
        //        });
        //    }
        //    var updateEnrolment = new UpdateEnrolment()
        //    {

        //        EnrolmentStatus = rejectedUserData.Decision,
        //        Comments = model.Comments
        //    };

        //    var result = await _facadeService.UpdateEnrolment(updateEnrolment);

        //    if (result == EndpointResponseStatus.Success)
        //    {
        //        rejectedUserData.ResponseStatus = EndpointResponseStatus.Success;

        //        var request = new EnrolmentDecisionRequest
        //        {
        //            OrganisationNumber = organisationDetails.OrganisationReferenceNumber,
        //            OrganisationName = organisationDetails.OrganisationName,
        //            ApprovedUser = approvedUserEmailDetails,
        //            DelegatedUsers = delegateUsersEmailDetails,
        //            RejectionComments = model.Comments,
        //            RegulatorRole = rejectedUserData.ServiceRole,
        //            Decision = rejectedUserData.Decision
        //        };

        //        await _facadeService.SendEnrolmentEmails(request);
        //    }
        //    else
        //    {
        //        rejectedUserData.ResponseStatus = EndpointResponseStatus.Fail;
        //    }

        //    TempData["RejectResult"] = rejectedUserData.ResponseStatus;
        //    TempData["RejectUserName"] = $"{rejectedUserData.FirstName} {rejectedUserData.LastName}";
        //    TempData["RejectedRole"] = rejectedUserData.ServiceRole;

        //    if (rejectedUserData.ServiceRole == ServiceRole.ApprovedPerson)
        //    {
        //        return await SaveSessionAndRedirect(
        //            session, nameof(Applications), PagePath.EnrolmentDecision, PagePath.Applications, null);
        //    }

        //    return await SaveSessionAndRedirect(
        //        session, nameof(EnrolmentRequests), PagePath.Applications, PagePath.EnrolmentRequests, null);
        //}

        public async Task<IActionResult> AcceptUserDetailChanges(ManageUserDetailsChangeRequest request)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new();

            await SaveSessionAndJourney(session, PagePath.UserDetailChangesRequest, PagePath.ManageUserDetailChanges);

            SetBackLink(session, PagePath.ManageUserDetailChanges);

            var acceptOrRejectResponse = await _facadeService.AcceptOrRejectUserDetailChangeRequest(request);

            try
            {
                if (acceptOrRejectResponse.HasUserDetailsChangeAccepted )
                {
                    var updateEnrolmentRequest = new UpdateEnrolment()
                    {

                        EnrolmentStatus = RegulatorDecision.Approved,
                        Comments = string.Empty
                    };

                    var result = await _facadeService.UpdateEnrolment(updateEnrolmentRequest);
                    if (result == EndpointResponseStatus.Success)
                    {
                      //  session.RegulatorManageUserDetailChangeSession.OrganisationId = organisationDetails.OrganisationId;

                        //TempData["AcceptResult"] = EndpointResponseStatus.Success;
                        //TempData["AcceptFirstName"] = acceptedUserFirstName;
                        //TempData["AcceptLastName"] = acceptedUserLastName;
                        //TempData["AcceptedRole"] = serviceRole;

                        await SaveSession(session);

                        return RedirectToAction(PagePath.UserDetailChangesRequest, "UserDetailChanges");
                    }
                }
            }
            catch (Exception e)
            {
               // _logger.LogError(e, $"Error accepting application for: {acceptedUserEmail} in organisation {organisationDetails.OrganisationId}");
            }
            ModelState.AddModelError("Update failed", "Update failed");

            //var enrolmentRequestsViewModel = GetEnrolmentRequestsViewModel(organisationDetails);

            //return View(nameof(UserDetailChangesRequest), enrolmentRequestsViewModel);
            return View(nameof(UserDetailChangesRequest), new UserDetailChangesRequestViewModel()); 

        }

        private static void ClearFilters(JourneySession session)
        {
            session.RegulatorManageUserDetailChangeSession.SearchOrganisationName = string.Empty;
            session.RegulatorManageUserDetailChangeSession.IsApprovedUserTypeChecked = false;
            session.RegulatorManageUserDetailChangeSession.IsDelegatedUserTypeChecked = false;
            session.RegulatorManageUserDetailChangeSession.CurrentPageNumber = 1;
        }

        private static void SetFilterValues(JourneySession session,
            string searchOrganisationName,
            bool isApprovedUserTypeChecked,
            bool isDelegatedUserTypeChecked,
            bool clearFilters,
            bool isFilteredSearch)
        {
            var regulatorManageUserDetailChangeSession = session.RegulatorManageUserDetailChangeSession;
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
                    regulatorManageUserDetailChangeSession.SearchOrganisationName = searchOrganisationName;
                    regulatorManageUserDetailChangeSession.IsApprovedUserTypeChecked = isApprovedUserTypeChecked;
                    regulatorManageUserDetailChangeSession.IsDelegatedUserTypeChecked = isDelegatedUserTypeChecked;
                    regulatorManageUserDetailChangeSession.CurrentPageNumber = 1;
                }
            }
        }
    }
}