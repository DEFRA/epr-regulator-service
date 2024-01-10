using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.RemoveApprovedUser;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Core.Models;

namespace EPR.RegulatorService.Frontend.Web.Controllers.RemoveApprovedUser;

using ViewModels.ApprovedPersonListPage;

[FeatureGate(FeatureFlags.ManageApprovedUsers)]
public class RemoveApprovedUserController : RegulatorSessionBaseController
{
    private readonly IFacadeService _facadeService;
    public RemoveApprovedUserController(ISessionManager<JourneySession> sessionManager,
        ILogger<RemoveApprovedUserController> logger,
        IConfiguration configuration,
        IFacadeService facadeService) : base(sessionManager, logger, configuration)
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
            && !string.IsNullOrWhiteSpace(userName)
            && !string.IsNullOrWhiteSpace(organisationName))
        {
            session.AddRemoveApprovedUserSession = new AddRemoveApprovedUserSession
            {
                UserNameToDelete = userName,
                ConnExternalId = connExternalId,
                NominationDecision = null,
                OrganisationName = organisationName,
                OrganisationId = organisationId,
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
            session.RegulatorSession.OrganisationId = model.OrganisationId;
            session.RegulatorSession.ConnExternalId = model.ConnExternalId;
            session.RegulatorSession.NominationDecision = model.NominationDecision;
        }

        //This is the flow when nominationDecision = true as of now it will stay on the same page
        if (model.NominationDecision ?? true)
        {
            return RedirectToAction("ConfirmNominationDecision", session.AddRemoveApprovedUserSession);
        }

        var request = new RemoveApprovedUserRequest
        {
            ConnectionExternalId = model.ConnExternalId,
            OrganisationId = model.OrganisationId,
            NominationDecision = (bool)model.NominationDecision
        };

        var response = await _facadeService.RemoveApprovedUser(request);
        session.AddRemoveApprovedUserSession.ResponseStatus = response;

        if (response == EndpointResponseStatus.Success)
        {
            return View("RemovedConfirmation", session.AddRemoveApprovedUserSession);
        }

        return RedirectToAction(PagePath.Error, "Error");
    }


    [HttpGet]
    [Route(PagePath.ApprovedPersonListPage)]
    public async Task<ActionResult> ApprovedPersonListPage(Guid externalOrganisationId)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new JourneySession();
        if (externalOrganisationId != Guid.Empty)
        {
            session.AddRemoveApprovedUserSession = new ();
            session.AddRemoveApprovedUserSession.OrganisationId = externalOrganisationId;
        }
        var results = await _facadeService.GetProducerOrganisationUsersByOrganisationExternalId(session.AddRemoveApprovedUserSession.OrganisationId);

        var model = new OrganisationUsersModel
        {
            ExternalOrganisationId = session.AddRemoveApprovedUserSession.OrganisationId,
            OrganisationUsers = results.Select(user => new OrganisationUser
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PersonExternalId = user.PersonExternalId
            }).ToList(),
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
            var selectedUser = results.FirstOrDefault(r => r.PersonExternalId == model.NewApprovedUserId);
            if (selectedUser != null)
            {
                session.AddRemoveApprovedUserSession.NewApprovedUser = selectedUser;
                _sessionManager.SaveSessionAsync(HttpContext.Session, session);

                return RedirectToAction("ConfirmNominationDecision");
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