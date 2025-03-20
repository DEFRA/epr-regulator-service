using EPR.RegulatorService.Frontend.Core.Extensions;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
using Microsoft.AspNetCore.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers;

public abstract class RegulatorSessionBaseController : Controller
{
    protected readonly ISessionManager<JourneySession> _sessionManager;
    private readonly string _pathBase;

    protected RegulatorSessionBaseController(
        ISessionManager<JourneySession> sessionManager,
        IConfiguration configuration)
    {
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        ArgumentNullException.ThrowIfNull(configuration);
        _pathBase = configuration.GetValue<string>(ConfigKeys.PathBase);
    }

    protected string GetHomeBackLink()
    {
        string pathBase = _pathBase.TrimStart('/').TrimEnd('/');
        return $"/{pathBase}/{PagePath.Home}";
    }

    protected async Task<RedirectToActionResult> SaveSessionAndRedirect(
        JourneySession session,
        string actionName,
        string currentPagePath,
        string? nextPagePath,
        object? routeValues)
    {
        await SaveSessionAndJourney(session, currentPagePath, nextPagePath);

        return RedirectToAction(actionName, routeValues);
    }

    // TODO: Can we change the existing version or should we have a separate overload?
    protected async Task SaveSessionAndJourney(JourneySession session, string currentPagePath)
    {
        session.RegulatorSession.Journey.AddIfNotExists(currentPagePath);

        ClearRestOfJourney(session, currentPagePath);

        await SaveSession(session);
    }

    //protected async Task SaveSessionAndJourney(JourneySession session, string currentPagePath, string? nextPagePath = null)
    //{
    //    session.RegulatorSession.Journey.AddIfNotExists(currentPagePath);

    //    ClearRestOfJourney(session, currentPagePath);

    //    if (nextPagePath != null)
    //    {
    //        session.RegulatorSession.Journey.AddIfNotExists(nextPagePath);
    //    }

    //    await SaveSession(session);
    //}

    protected async Task SaveSessionAndJourney(JourneySession session, string currentPagePath, string? nextPagePath)
    {
        ClearRestOfJourney(session, currentPagePath);

        session.RegulatorSession.Journey.AddIfNotExists(currentPagePath);
        session.RegulatorSession.Journey.AddIfNotExists(nextPagePath);
        
        await SaveSession(session);
    }

    protected async Task AddCurrentPageAndSaveSession(JourneySession session, string currentPagePath)
    {
        session.RegulatorSession.Journey.AddIfNotExists(currentPagePath);

        ClearRestOfJourney(session, currentPagePath);

        await SaveSession(session);
    }


    private static void ClearRestOfJourney(JourneySession session, string currentPagePath)
    {
        int index = session.RegulatorSession.Journey.IndexOf(currentPagePath);

        // this also cover if current page not found (index = -1) then it clears all pages
        session.RegulatorSession.Journey = session.RegulatorSession.Journey.Take(index + 1).ToList();
    }

    protected async Task SaveSession(JourneySession session) =>
        await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

    protected void SetBackLink(JourneySession session, string currentPagePath) =>
        ViewBag.BackLinkToDisplay =
            session.RegulatorSession.Journey.PreviousOrDefault(currentPagePath) ?? string.Empty;

    protected void SetBackLinkAriaLabel()
    {
        ViewBag.BackLinkAriaLabel = "Click here if you wish to go back to the previous page";//will be added to localizer
    }
    protected void SetHomeBackLink()
    {
        string pathBase = _pathBase.TrimStart('/').TrimEnd('/');
        ViewBag.BackLinkToDisplay = $"/{pathBase}/{PagePath.Home}";
    }
}