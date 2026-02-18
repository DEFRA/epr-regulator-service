namespace EPR.Common.Functions.Database.UnitOfWork;

using CancellationTokens.Interfaces;
using Context.Interfaces;
using Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly IEprCommonContext context;
    private readonly ICancellationTokenAccessor cancellationTokenAccessor;

    public UnitOfWork(IEprCommonContext context, ICancellationTokenAccessor cancellationTokenAccessor)
    {
        this.context = context;
        this.cancellationTokenAccessor = cancellationTokenAccessor;
    }

    public int SaveChanges() => this.context.SaveChanges();

    public async Task<int> SaveChangesAsync() => await this.SaveContextChangesAsync();

    public T Execute<T>(Func<T> impl, bool saveOnException = false)
    {
        try
        {
            var result = impl();
            this.SaveChanges();
            return result;
        }
        catch
        {
            if (saveOnException)
            {
                this.SaveChanges();
            }

            throw;
        }
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> impl, bool saveOnException = false)
    {
        try
        {
            var result = await impl();
            await this.SaveContextChangesAsync();
            return result;
        }
        catch
        {
            if (saveOnException)
            {
                await this.SaveContextChangesAsync();
            }

            throw;
        }
    }

    public int Execute(Action impl, bool saveOnException = false)
    {
        try
        {
            impl();
            return this.SaveChanges();
        }
        catch
        {
            if (saveOnException)
            {
                this.SaveChanges();
            }

            throw;
        }
    }

    public async Task<int> ExecuteAsync(Func<Task> impl, bool saveOnException = false)
    {
        try
        {
            await impl();
            return await this.SaveContextChangesAsync();
        }
        catch
        {
            if (saveOnException)
            {
                await this.SaveContextChangesAsync();
            }

            throw;
        }
    }

    public IDbContextTransaction BeginTransaction() => this.context.Database.BeginTransaction();

    public async Task<IDbContextTransaction> BeginTransactionAsync() => await this.context.Database.BeginTransactionAsync(this.cancellationTokenAccessor.CancellationToken);

    public void TryAppLock(Guid lockId)
    {
        // Syntax isn't supported by SQLite in unit tests
        if (this.context.Database.IsSqlServer())
        {
            // Acquire a lock for this Id. This lock is tied to the transaction and automatically freed on commit, rollback or connection drop.
            this.context.Database.ExecuteSqlInterpolated(this.CreateAppLockSql(lockId));
        }
    }

    public async Task TryAppLockAsync(Guid lockId)
    {
        // Syntax isn't supported by SQLite in unit tests
        if (this.context.Database.IsSqlServer())
        {
            // Acquire a lock for this Id. This lock is tied to the transaction and automatically freed on commit, rollback or connection drop.
            await this.context.Database.ExecuteSqlInterpolatedAsync(this.CreateAppLockSql(lockId));
        }
    }

    private async Task<int> SaveContextChangesAsync() => await this.context.SaveChangesAsync(this.cancellationTokenAccessor.CancellationToken);

    // Uses FormattableString as ExecuteSqlInterpolated will translate to parameterised SQL (lockId will be passed as a DbParameter)
    private FormattableString CreateAppLockSql(Guid lockId) => @$"
			        declare @result int;
			        EXEC @result = sp_getapplock {lockId}, 'Exclusive', 'Transaction', 10000 ;
			        IF @result < 0
				        RAISERROR('ERROR: cannot get the lock [{lockId}] in less than 10 seconds.', 16, 1);
			        ";
}