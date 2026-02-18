namespace EPR.Common.Authorization.Services.Interfaces;

public interface IGraphService
{
    Task PatchUserProperty(Guid userId, string propertyName, string value,
        CancellationToken cancellationToken = default);

    Task<string?> QueryUserProperty(Guid userId, string propertyName, 
        CancellationToken cancellationToken = default);
}
