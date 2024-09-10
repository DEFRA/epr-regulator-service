using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.InviteNewApprovedPerson;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Services;

namespace EPR.RegulatorService.Frontend.Web.Controllers.InviteNewApprovedPerson;

[Route(PagePath.InviteNewApprovedPersonPage)]
[FeatureGate(FeatureFlags.ManageApprovedUsers)]
public class InviteNewApprovedPersonController : RegulatorSessionBaseController
{
    private readonly IFacadeService _facadeService;

    public InviteNewApprovedPersonController(
        ISessionManager<JourneySession> sessionManager,
        IConfiguration configuration,
        IFacadeService facadeService)
        : base(sessionManager, configuration)
    {
        _facadeService = facadeService;
    }

    [HttpGet]
    [Route(PagePath.InviteNewApprovedPersonName)]
    public async Task<ActionResult> EnterPersonName()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new JourneySession();
        session.InviteNewApprovedPersonSession = new InviteNewApprovedPersonSession
        {
            ExternalOrganisationId = session.AddRemoveApprovedUserSession.ExternalOrganisationId,
            OrganisationName = session.AddRemoveApprovedUserSession.OrganisationName,
            HasANewApprovedPersonBeenNominated = session.AddRemoveApprovedUserSession.NominationDecision,
            RemovedConnectionExternalId = session.AddRemoveApprovedUserSession.ConnExternalId,
            UserNameToRemove = session.AddRemoveApprovedUserSession.UserNameToRemove,
            InvitedPersonFirstname = session.InviteNewApprovedPersonSession?.InvitedPersonFirstname ?? string.Empty,
            InvitedPersonLastname =  session.InviteNewApprovedPersonSession?.InvitedPersonLastname ?? string.Empty,
            InvitedPersonEmail = session.InviteNewApprovedPersonSession?.InvitedPersonEmail ?? string.Empty
        };

        await AddCurrentPageAndSaveSession(session, PagePath.InviteNewApprovedPersonName);

        ViewBag.BackLinkToDisplay = Url.Action("ApprovedPersonListPage", "RemoveApprovedUser");
        return View(new EnterPersonNameModel
        {
            FirstName = session.InviteNewApprovedPersonSession.InvitedPersonFirstname,
            LastName = session.InviteNewApprovedPersonSession.InvitedPersonLastname
        });
    }

    [HttpPost]
    [Route(PagePath.InviteNewApprovedPersonName)]
    public async Task<ActionResult> EnterPersonName(EnterPersonNameModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new JourneySession();
        session.InviteNewApprovedPersonSession.InvitedPersonFirstname = model.FirstName;
        session.InviteNewApprovedPersonSession.InvitedPersonLastname = model.LastName;
        await AddCurrentPageAndSaveSession(session, PagePath.InviteNewApprovedPersonName);

        if (ModelState.IsValid)
        {
            return RedirectToAction("EnterPersonEmail");
        }

        SetBackLink(session, PagePath.InviteNewApprovedPersonName);
        return View(model);
    }

    [HttpGet]
    [Route(PagePath.InviteNewApprovedPersonEmail)]
    public async Task<ActionResult> EnterPersonEmail()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new JourneySession();
        await AddCurrentPageAndSaveSession(session, PagePath.InviteNewApprovedPersonEmail);
        SetBackLink(session, PagePath.InviteNewApprovedPersonEmail);
        return View(new EnterPersonEmailModel
        {
            Email = session.InviteNewApprovedPersonSession?.InvitedPersonEmail
        });
    }

    [HttpPost]
    [Route(PagePath.InviteNewApprovedPersonEmail)]
    public async Task<ActionResult> EnterPersonEmail(EnterPersonEmailModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new JourneySession();
        session.InviteNewApprovedPersonSession.InvitedPersonEmail = model.Email;
        await AddCurrentPageAndSaveSession(session, PagePath.InviteNewApprovedPersonEmail);
        SetBackLink(session, PagePath.InviteNewApprovedPersonEmail);
        if (ModelState.IsValid)
        {
            return RedirectToAction("Confirmation");
        }


        return View(model);
    }

    [HttpGet]
    [Route(PagePath.InviteNewApprovedPersonConfirmation)]
    public async Task<ActionResult> Confirmation()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new JourneySession();

        var model = new ConfirmationModel
        {
            InvitedApprovedPersonEmail = session.InviteNewApprovedPersonSession.InvitedPersonEmail,
            InvitedApprovedPersonFullName = session.InviteNewApprovedPersonSession.InvitedPersonFullName,
            HasANewApprovedPersonBeenNominated = session.InviteNewApprovedPersonSession.HasANewApprovedPersonBeenNominated,
        };

        await AddCurrentPageAndSaveSession(session, PagePath.InviteNewApprovedPersonConfirmation);
        SetBackLink(session, PagePath.InviteNewApprovedPersonConfirmation);
        return View(model);
    }

    [HttpPost]
    [HttpGet]
    [Route(PagePath.InviteNewApprovedPersonSubmit)]
    public async Task<ActionResult> Submit()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new JourneySession();

        var request = new AddRemoveApprovedUserRequest
        {
            OrganisationId = session.InviteNewApprovedPersonSession.ExternalOrganisationId,
            RemovedConnectionExternalId = session.InviteNewApprovedPersonSession.RemovedConnectionExternalId,
            InvitedPersonEmail = session.InviteNewApprovedPersonSession.InvitedPersonEmail,
            InvitedPersonFirstname = session.InviteNewApprovedPersonSession.InvitedPersonFirstname,
            InvitedPersonLastname = session.InviteNewApprovedPersonSession.InvitedPersonLastname
        };

        var response = await _facadeService.AddRemoveApprovedUser(request);
        session.InviteNewApprovedPersonSession.ResponseStatus = response;

        if (response == EndpointResponseStatus.Success)
        {
            if (session.InviteNewApprovedPersonSession.RemovedConnectionExternalId is null
                || session.InviteNewApprovedPersonSession.RemovedConnectionExternalId == Guid.Empty)
            {
                var emailSentToNominatedApprovedPersonModel = new EmailSentToNominatedApprovedPersonModel
                {
                    InvitedApprovedPersonFullName = $"{session.InviteNewApprovedPersonSession.InvitedPersonFirstname} {session.InviteNewApprovedPersonSession.InvitedPersonLastname}",
                    OrganisationName = session.InviteNewApprovedPersonSession.OrganisationName
                };
                return View("EmailSentToNominatedApprovedPerson", emailSentToNominatedApprovedPersonModel);
            }

            var accountPermissionHaveChangedModel = new AccountPermissionHaveChangedModel
            {
                InvitedPersonFullName =
                    $"{session.InviteNewApprovedPersonSession.InvitedPersonFirstname} {session.InviteNewApprovedPersonSession.InvitedPersonLastname}",
                OrganisationName = session.InviteNewApprovedPersonSession.OrganisationName,
                RemovedPersonFullName = session.InviteNewApprovedPersonSession.UserNameToRemove
            };

            return View("AccountPermissionHaveChanged", accountPermissionHaveChangedModel);
        }
        return RedirectToAction(PagePath.Error, "Error");
    }
}