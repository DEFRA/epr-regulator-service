using EPR.RegulatorService.Frontend.Core.Exceptions;
using EPR.RegulatorService.Frontend.Core.Extensions;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
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

    protected async Task SaveSessionAndJourney(JourneySession session, string currentPagePath)
    {
        session.RegulatorSession.Journey.AddIfNotExists(currentPagePath);

        ClearRestOfJourney(session, currentPagePath);

        await SaveSession(session);
    }

    protected void SetBackLinkInfos(JourneySession session, string currentPagePath)
    {
        if (string.IsNullOrEmpty(Request?.Headers?.Referer))
            SetHomeBackLink();
        else
            SetBackLink(session, currentPagePath);

        SetBackLinkAriaLabel();
    }

    protected static void InitialiseAccreditationStatusSessionIfNotExists(JourneySession session, Guid accreditationId, int year)
    {
        if (session.ReprocessorExporterSession.AccreditationStatusSession != null &&
            session.ReprocessorExporterSession.AccreditationStatusSession!.AccreditationId == accreditationId &&
            session.ReprocessorExporterSession.AccreditationStatusSession!.Year == year)
        {
            return;
        }

        var accreditationStatusSession = new AccreditationStatusSession
        {
            AccreditationId = accreditationId,
            Year = year,
            OrganisationName = null!,
            MaterialName = null!
        };

        session.ReprocessorExporterSession.AccreditationStatusSession = accreditationStatusSession;
    }


    protected static string GetRegistrationsView(string viewName) => $"~/Views/ReprocessorExporter/Registrations/{viewName}.cshtml";
}