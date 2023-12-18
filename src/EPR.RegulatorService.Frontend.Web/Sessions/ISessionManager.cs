namespace EPR.RegulatorService.Frontend.Web.Sessions;

public interface ISessionManager<T> where T : new()
{
    Task<T?> GetSessionAsync(ISession session);

    Task SaveSessionAsync(ISession session, T sessionValue);

    void RemoveSession(ISession session);

    Task UpdateSessionAsync(ISession session, Action<T> updateFunc);
}
