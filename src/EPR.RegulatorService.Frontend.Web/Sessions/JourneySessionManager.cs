using EPR.RegulatorService.Frontend.Core.Sessions;

using System.Text.Json;

namespace EPR.RegulatorService.Frontend.Web.Sessions;

public class JourneySessionManager : ISessionManager<JourneySession>
{
    private readonly string _sessionKey = nameof(JourneySession);

    private JourneySession? _sessionValue;

    public async Task<JourneySession?> GetSessionAsync(ISession session)
    {
        if (_sessionValue != null)
        {
            return _sessionValue;
        }

        await session.LoadAsync();

        string? sessionString = session.GetString(_sessionKey);

        if (sessionString != null)
        {
            _sessionValue = JsonSerializer.Deserialize<JourneySession>(sessionString);
        }

        return _sessionValue;
    }

    public void RemoveSession(ISession session)
    {
        session.Remove(_sessionKey);

        _sessionValue = null;
    }

    public async Task SaveSessionAsync(ISession session, JourneySession sessionValue)
    {
        await session.LoadAsync();

        session.SetString(_sessionKey, JsonSerializer.Serialize(sessionValue));

        _sessionValue = sessionValue;
    }

    public async Task UpdateSessionAsync(ISession session, Action<JourneySession> updateFunc)
    {
        var sesionValue = await GetSessionAsync(session) ?? new JourneySession();

        updateFunc(sesionValue);

        await SaveSessionAsync(session, sesionValue);
    }
}
