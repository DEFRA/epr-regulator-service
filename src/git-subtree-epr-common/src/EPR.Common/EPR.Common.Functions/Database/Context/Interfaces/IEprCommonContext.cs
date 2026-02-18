namespace EPR.Common.Functions.Database.Context.Interfaces;

using Microsoft.EntityFrameworkCore.Infrastructure;

public interface IEprCommonContext
{
    public DatabaseFacade Database { get; }

    public int SaveChanges();

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
}