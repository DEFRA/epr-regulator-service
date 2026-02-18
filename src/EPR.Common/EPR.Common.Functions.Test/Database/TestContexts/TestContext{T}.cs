namespace EPR.Common.Functions.Test.Database.TestContexts;

using Functions.AccessControl.Interfaces;
using Functions.Database.Context;
using Functions.Database.Decorators.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;
using System;
using System.Collections.Generic;

internal class TestContext<T> : EprCommonContext
    where T : class
{
    public TestContext(IRequestTimeService requestTimeService, IUserContextProvider userContextProvider, IEnumerable<IEntityDecorator> entityDecorators)
        : base(ContextOptions(), userContextProvider, requestTimeService, entityDecorators)
    {
    }

    public DbSet<T> Things { get; set; }

    protected override void ConfigureApplicationKeys(ModelBuilder modelBuilder)
    {
    }

    private static DbContextOptions<EprCommonContext> ContextOptions() => new DbContextOptionsBuilder<EprCommonContext>()
        .UseSqlite(new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = $"{typeof(T)}:{Guid.NewGuid()}",
            Mode = SqliteOpenMode.Memory,
            Cache = SqliteCacheMode.Shared,
        }.ToString()))
        .EnableSensitiveDataLogging()
        .EnableDetailedErrors()
        .Options;
}
