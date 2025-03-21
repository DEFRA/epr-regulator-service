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
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new JourneySession();

        return session;
    }

    protected async Task SaveSessionAndJourney(JourneySession session, string currentPagePath)
    {
        session.RegulatorSession.Journey.AddIfNotExists(currentPagePath);

        ClearRestOfJourney(session, currentPagePath);

        await SaveSession(session);
    }
}