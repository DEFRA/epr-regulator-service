using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.ApprovedPersonListPage;
using EPR.RegulatorService.Frontend.Web.ViewModels.RemoveApprovedUser;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Core.Models;

namespace EPR.RegulatorService.Frontend.Web.Controllers.RemoveApprovedUser;

[FeatureGate(FeatureFlags.ManageApprovedUsers)]
public class RemoveApprovedUserController : RegulatorSessionBaseController
{
    private readonly IFacadeService _facadeService;
    public RemoveApprovedUserController(ISessionManager<JourneySession> sessionManager,
        IConfiguration configuration,
        IFacadeService facadeService) : base(sessionManager, configuration)
    {
        _facadeService = facadeService;
    }

    [HttpGet]
    [Route(PagePath.RemoveApprovedUserConfirmationPage)]
    public async Task<IActionResult> Confirm(string userName, string organisationName, Guid connExternalId, Guid organisationId)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new();

        if (connExternalId != Guid.Empty
            && organisationId != Guid.Empty
            && !string.IsNullOrWhiteSpace(organisationName))
        {
            session.AddRemoveApprovedUserSession = new AddRemoveApprovedUserSession
            {
                UserNameToRemove = userName,
                ConnExternalId = connExternalId,
                NominationDecision = null,
                OrganisationName = organisationName,
                ExternalOrganisationId = organisationId,
                CurrentApprovedUserEmail = session.AddRemoveApprovedUserSession.CurrentApprovedUserEmail
            };
            await SaveSession(session);
        }

        await AddCurrentPageAndSaveSession(session, PagePath.RemoveApprovedUserConfirmationPage);
        SetBackLink(session, PagePath.RemoveApprovedUserConfirmationPage);

        return View(session.AddRemoveApprovedUserSession);
    }

    [HttpGet]
    [Route(PagePath.NominationDecision)]
    public async Task<IActionResult> NominationDecision()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new();

        await AddCurrentPageAndSaveSession(session, PagePath.NominationDecision);
        SetBackLink(session, PagePath.NominationDecision);


        return View(session.AddRemoveApprovedUserSession);
    }

    [HttpPost]
    [Route(PagePath.NominationDecision)]
    public async Task<IActionResult> Nomination(ApprovedUserToRemoveViewModel model)
    {
        const string backLink = PagePath.NominationDecision;
        ViewBag.BackLinkToDisplay = backLink;

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new();

        if (model.NominationDecision == null)
        {
            ModelState.AddModelError(nameof(session.AddRemoveApprovedUserSession.NominationDecision), "NominateApproved.Decision.Error");
            return View(nameof(NominationDecision));
        }

        session.AddRemoveApprovedUserSession.NominationDecision = model.NominationDecision;
        _sessionManager.SaveSessionAsync(HttpContext.Session, session);

        if (session.AddRemoveApprovedUserSession.NominationDecision == true)
        {
            return RedirectToAction("ApprovedPersonListPage");
        }
        return RedirectToAction("ConfirmNominationDecision");
    }

    [HttpGet]
    [Route(PagePath.NominationDecisionConfirmation)]
    public async Task<IActionResult> ConfirmNominationDecision()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new();

        await AddCurrentPageAndSaveSession(session, PagePath.NominationDecisionConfirmation);
        SetBackLink(session, PagePath.NominationDecisionConfirmation);

        return View("NominateApprovedUserConfirmation", session.AddRemoveApprovedUserSession);

    }

    [HttpPost]
    [Route(PagePath.NominationDecisionConfirmation)]
    public async Task<IActionResult> Submit(ApprovedUserToRemoveViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new JourneySession();
        if (model.ConnExternalId != Guid.Empty && model.OrganisationId != Guid.Empty)
        {
            session.AddRemoveApprovedUserSession.ExternalOrganisationId = model.OrganisationId;
            session.AddRemoveApprovedUserSession.ConnExternalId = model.ConnExternalId;
            session.AddRemoveApprovedUserSession.NominationDecision = model.NominationDecision;
        }

        var request = new RemoveApprovedUserRequest
        {
            RemovedConnectionExternalId = model.ConnExternalId,
            OrganisationId = model.OrganisationId,
            PromotedPersonExternalId = model.PromotedPersonExternalId
        };

        var response = await _facadeService.RemoveApprovedUser(request);
        session.AddRemoveApprovedUserSession.ResponseStatus = response;

        if (response == EndpointResponseStatus.Success)
        {
            //This is the nominate only flow (no removal)
            if (model.NominationDecision == null)
            {
                return await NominateOnly(session.AddRemoveApprovedUserSession);
            }

            //This is remove and nominate flow
            if (model.NominationDecision == true)
            {
                return await RemoveAndNominate(session.AddRemoveApprovedUserSession);
            }

            //This is the remove only flow (model.NominationDecision == false, ie. no nomination)
            return View("RemovedConfirmation", session.AddRemoveApprovedUserSession);
        }
        return RedirectToAction(PagePath.Error, "Error");
    }

    public async Task<IActionResult> NominateOnly(AddRemoveApprovedUserSession addRemoveApprovedUserSession)
    {
        var promotedApprovedUserModel = new RemovedNominatedUserViewModel()
        {
            PromotedUserName =
                $"{addRemoveApprovedUserSession.NewApprovedUser.FirstName} " +
                $"{addRemoveApprovedUserSession.NewApprovedUser.LastName}",
            OrganisationName = addRemoveApprovedUserSession.OrganisationName
        };
        return RedirectToAction("EmailNominatedApprovedPerson", promotedApprovedUserModel);
    }

    private async Task<IActionResult> RemoveAndNominate(AddRemoveApprovedUserSession addRemoveApprovedUserSession)
    {
        var promotedApprovedUserModel = new RemovedNominatedUserViewModel()
        {
            PromotedUserName =
                $"{addRemoveApprovedUserSession.NewApprovedUser.FirstName} " +
                $"{addRemoveApprovedUserSession.NewApprovedUser.LastName}",
            RemovedUserName = addRemoveApprovedUserSession.UserNameToRemove,
            OrganisationName = addRemoveApprovedUserSession.OrganisationName
        };
        return RedirectToAction("AccountPermissionsChanged", promotedApprovedUserModel);
    }


    [HttpGet]
    [Route(PagePath.EmailNominatedApprovedPerson)]
    public async Task<ActionResult> EmailNominatedApprovedPerson(RemovedNominatedUserViewModel model)
        => View("EmailSentToNominatedApprovedPerson", model);
    
    [HttpGet]
    [Route(PagePath.AccountPermissionsChanged)]
    public async Task<ActionResult> AccountPermissionsChanged(RemovedNominatedUserViewModel model)
        => View("AccountPermissionsHaveChanged", model);


    [HttpGet]
    [Route(PagePath.ApprovedPersonListPage)]
    public async Task<ActionResult> ApprovedPersonListPage()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new JourneySession();

        var results = await _facadeService.GetProducerOrganisationUsersByOrganisationExternalId(session.AddRemoveApprovedUserSession.ExternalOrganisationId);

        var model = new OrganisationUsersModel
        {
            ExternalOrganisationId = session.AddRemoveApprovedUserSession.ExternalOrganisationId,
            OrganisationUsers = results.Where(user => user.Email != session.AddRemoveApprovedUserSession.CurrentApprovedUserEmail).ToList(),
            NewApprovedUserId = session.AddRemoveApprovedUserSession.NewApprovedUser?.PersonExternalId
        };

        await AddCurrentPageAndSaveSession(session, PagePath.ApprovedPersonListPage);
        SetBackLink(session, PagePath.ApprovedPersonListPage);

        return View(model);
    }

    [HttpPost]
    [Route(PagePath.ApprovedPersonListPage)]
    public async Task<ActionResult> ApprovedPersonListPage(OrganisationUsersModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new JourneySession();

        var results = await _facadeService.GetProducerOrganisationUsersByOrganisationExternalId(model.ExternalOrganisationId);

        if (ModelState.IsValid)
        {
            if (model.NewApprovedUserId == Guid.Empty)
            {
                session.InviteNewApprovedPersonSession = new InviteNewApprovedPersonSession();
                await AddCurrentPageAndSaveSession(session, PagePath.ApprovedPersonListPage);
                return RedirectToAction("EnterPersonName", "InviteNewApprovedPerson");
            }

            var selectedUser = results.Find(r => r.PersonExternalId == model.NewApprovedUserId);
            if (selectedUser != null)
            {
                session.AddRemoveApprovedUserSession.NewApprovedUser = selectedUser;
                _sessionManager.SaveSessionAsync(HttpContext.Session, session);

                return RedirectToAction("ConfirmNominationDecision", session.AddRemoveApprovedUserSession);
            }
        }

        model.OrganisationUsers = results.Select(user => new OrganisationUser
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PersonExternalId = user.PersonExternalId
        }).ToList();

        await AddCurrentPageAndSaveSession(session, PagePath.ApprovedPersonListPage);
        SetBackLink(session, PagePath.ApprovedPersonListPage);
        return View(model);
    }
}