namespace EPR.Common.Authorization.Sessions;

using Microsoft.AspNetCore.Http;

public interface ISessionManager<T>
{
    Task<T?> GetSessionAsync(ISession session);

    Task SaveSessionAsync(ISession session, T sessionValue);

    void RemoveSession(ISession session);

    Task UpdateSessionAsync(ISession session, Action<T> updateFunc);
}