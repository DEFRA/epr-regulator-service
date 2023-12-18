using EPR.RegulatorService.Frontend.Core.Sessions;
using System.Text.Json;

namespace EPR.RegulatorService.Frontend.Web.Sessions;

public class RegulatorSessionManager : ISessionManager<RegulatorSession>
{
    private readonly string _sessionKey = nameof(EPR.RegulatorService.Frontend.Core.Sessions.RegulatorSession);

    private EPR.RegulatorService.Frontend.Core.Sessions.RegulatorSession? _sessionValue;

    public async Task<EPR.RegulatorService.Frontend.Core.Sessions.RegulatorSession?> GetSessionAsync(ISession session)
    {
        if (_sessionValue != null)
        {
            return _sessionValue;
        }

        await session.LoadAsync();

        var sessionString = session.GetString(_sessionKey);

        if (sessionString != null)
        {
            _sessionValue = JsonSerializer.Deserialize<EPR.RegulatorService.Frontend.Core.Sessions.RegulatorSession>(sessionString);
        }

        return _sessionValue;
    }

    public void RemoveSession(ISession session)
    {
        session.Remove(_sessionKey);

        _sessionValue = null;
    }

    public async Task SaveSessionAsync(ISession session, EPR.RegulatorService.Frontend.Core.Sessions.RegulatorSession sessionValue)
    {
        await session.LoadAsync();

        session.SetString(_sessionKey, JsonSerializer.Serialize(sessionValue));

        _sessionValue = sessionValue;
    }

    public async Task UpdateSessionAsync(ISession session, Action<EPR.RegulatorService.Frontend.Core.Sessions.RegulatorSession> updateFunc)
    {
        var sesionValue = await GetSessionAsync(session) ?? new EPR.RegulatorService.Frontend.Core.Sessions.RegulatorSession();

        updateFunc(sesionValue);

        await SaveSessionAsync(session, sesionValue);
    }
}
