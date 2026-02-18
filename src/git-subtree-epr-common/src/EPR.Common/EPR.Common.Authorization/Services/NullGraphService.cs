using EPR.Common.Authorization.Services.Interfaces;

namespace EPR.Common.Authorization.Services;

public class NullGraphService : IGraphService
{
    public static readonly NullGraphService Empty = new();

    public Task PatchUserProperty(Guid userId, string propertyName, string value, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public async Task<string?> QueryUserProperty(Guid userId, string propertyName, CancellationToken cancellationToken = default)
    {
        return null;
    }
}