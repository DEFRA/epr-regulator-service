using EPR.RegulatorService.Frontend.Core.Exceptions;
using EPR.RegulatorService.Frontend.Core.Extensions;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Sessions;

namespace EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter;

public abstract class ReprocessorExporterBaseController(
    ISessionManager<JourneySession> sessionManager,
    IConfiguration configuration)
    : RegulatorSessionBaseController(sessionManager, configuration)
{
    protected async Task<JourneySession> GetSession()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (session == null)
        {
            throw new SessionException("Session not found");
        }

        return session;
    }
    
    protected async Task SaveCurrentPageToSession(JourneySession session)
    {
        string currentPagePath = GetRelativeUrlWithQueryString();
        session.RegulatorSession.Journey.AddIfNotExists(currentPagePath);

        ClearRestOfJourney(session, currentPagePath);

        await SaveSession(session);
    }

    protected void SetBackLinkInfos(JourneySession session)
    {
        if (string.IsNullOrEmpty(Request?.Headers?.Referer))
        {
            SetHomeBackLink();
        }
        else
        {
            string currentPagePath = GetRelativeUrlWithQueryString();
            SetBackLink(session, currentPagePath);
        }

        SetBackLinkAriaLabel();
    }
    
    protected static string GetRegistrationsView(string viewName) => $"~/Views/ReprocessorExporter/Registrations/{viewName}.cshtml";

    protected string GetRelativeUrlWithQueryString()
    {
        string relativeUrl = HttpContext.Request.Path.Value ?? string.Empty;
        string queryString = HttpContext.Request.QueryString.HasValue ? HttpContext.Request.QueryString.Value : string.Empty;

        return $"{relativeUrl}{queryString}";
    }
}