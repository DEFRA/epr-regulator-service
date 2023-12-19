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

[Route(PagePath.RemoveApprovedUserPage)]
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
        if (string.IsNullOrEmpty(session.RemoveApprovedUserSession.OrganisationName)
            || string.IsNullOrEmpty(session.RemoveApprovedUserSession.UserNameToDelete)
            || Guid.Empty == connExternalId
            || Guid.Empty == organisationId)
        {
            session.RemoveApprovedUserSession = new RemoveApprovedUserSession
            {
                UserNameToDelete = userName,
                ConnExternalId = connExternalId,
                NominationDecision = null,
                OrganisationName = organisationName,
                OrganisationId = organisationId,
            };
            await SaveSession(session);
        }

        ViewBag.BackLinkToDisplay = GetHomeBackLink();

        return View(session.RemoveApprovedUserSession);
    }

    [HttpPost]
    [Route(PagePath.RemoveApprovedUserConfirmationPage)]
    public IActionResult Continue() => RedirectToAction("NominationDecision");

    [HttpGet]
    [Route(PagePath.NominationDecision)]
    public async Task<IActionResult> NominationDecision()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new();

        string backLink = $"{PagePath.RemoveApprovedUserConfirmationPage}?userName={session.RemoveApprovedUserSession.UserNameToDelete}" +
                          $"&organisationName={session.RemoveApprovedUserSession.OrganisationName}" +
                          $"&connExternalId={session.RemoveApprovedUserSession.ConnExternalId}";

        ViewBag.BackLinkToDisplay = backLink;

        return View(nameof(NominationDecision));
    }

    [HttpPost]
    [Route(PagePath.NominationDecision)]
    public async Task<IActionResult> Nomination(bool? nominationDecision)
    {
        const string backLink = PagePath.NominationDecision;
        ViewBag.BackLinkToDisplay = backLink;

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new();

        if (nominationDecision == null)
        {
            ModelState.AddModelError(nameof(session.RemoveApprovedUserSession.NominationDecision), "NominateApproved.Decision.Error");
            return View(nameof(NominationDecision));
        }

        session.RemoveApprovedUserSession.NominationDecision = nominationDecision;

        await SaveSession(session);

        return RedirectToAction("ConfirmNominationDecision", session.RemoveApprovedUserSession);
    }

    [HttpGet]
    [Route(PagePath.NominationDecisionConfirmation)]
    public async Task<IActionResult> ConfirmNominationDecision()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new();

        const string backLink = PagePath.NominationDecision;

        ViewBag.BackLinkToDisplay = backLink;

        return View("NominateApprovedUserConfirmation", session.RemoveApprovedUserSession);
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
        }

        var response = await _facadeService.RemoveApprovedUser(
            session.RegulatorSession.ConnExternalId.Value,
            session.RegulatorSession.OrganisationId.Value);

        if (response == EndpointResponseStatus.Success)
        {
            return View("RemovedConfirmation", session.RemoveApprovedUserSession);
        }

        return RedirectToAction(PagePath.Error, "Error");
    }
}