namespace EPR.Common.Authorization.Sessions;

using System.Text.Json;
using Microsoft.AspNetCore.Http;

public class SessionManager<T> : ISessionManager<T>
    where T : class, new()
{
    private readonly string _sessionKey = typeof(T).Name;

    private T? _sessionValue;

    public async Task<T?> GetSessionAsync(ISession session)
    {
        if (_sessionValue != null)
        {
            return _sessionValue;
        }

        await session.LoadAsync();

        var sessionString = session.GetString(_sessionKey);

        if (sessionString != null)
        {
            _sessionValue = JsonSerializer.Deserialize<T>(sessionString);
        }

        return _sessionValue;
    }

    public void RemoveSession(ISession session)
    {
        session.Remove(_sessionKey);

        _sessionValue = null;
    }

    public async Task SaveSessionAsync(ISession session, T sessionValue)
    {
        await session.LoadAsync();

        session.SetString(_sessionKey, JsonSerializer.Serialize(sessionValue));
        
        _sessionValue = sessionValue;
    }

    public async Task UpdateSessionAsync(ISession session, Action<T> updateFunc)
    {
        var sessionValue = await GetSessionAsync(session) ?? new T();

        updateFunc(sessionValue);

        await SaveSessionAsync(session, sessionValue);
    }
}