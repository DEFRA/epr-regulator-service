namespace EPR.Common.Functions.Database.UnitOfWork.Interfaces;

using Microsoft.EntityFrameworkCore.Storage;

public interface IUnitOfWork
{
    public int SaveChanges();

    public Task<int> SaveChangesAsync();

    public T Execute<T>(Func<T> impl, bool saveOnException = false);

    public Task<T> ExecuteAsync<T>(Func<Task<T>> impl, bool saveOnException = false);

    public int Execute(Action impl, bool saveOnException = false);

    public Task<int> ExecuteAsync(Func<Task> impl, bool saveOnException = false);

    public IDbContextTransaction BeginTransaction();

    public Task<IDbContextTransaction> BeginTransactionAsync();

    public void TryAppLock(Guid lockId);

    public Task TryAppLockAsync(Guid lockId);
}