namespace EPR.Common.Functions.Test.Database.TestContexts;

using EPR.Common.Functions.Database.Context;
using EPR.Common.Functions.Database.Decorators.Interfaces;
using EPR.Common.Functions.Services.Interfaces;
using Functions.AccessControl.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

internal class TestContext<T1, T2> : EprCommonContext
    where T1 : class
    where T2 : class
{
    public TestContext(IUserContextProvider userContextProvider, IRequestTimeService requestTimeService, IEnumerable<IEntityDecorator> entityDecorators)
    : base(ContextOptions(), userContextProvider, requestTimeService, entityDecorators)
    {
    }

    public DbSet<T1> Things { get; set; }

    public DbSet<T2> ThingAudits { get; set; }

    protected override void ConfigureApplicationKeys(ModelBuilder modelBuilder)
    {
    }

    private static DbContextOptions<EprCommonContext> ContextOptions() => new DbContextOptionsBuilder<EprCommonContext>()
        .UseSqlite(new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = $"{typeof(T1)}:{typeof(T2)}:{Guid.NewGuid()}",
            Mode = SqliteOpenMode.Memory,
            Cache = SqliteCacheMode.Shared,
        }.ToString()))
        .EnableSensitiveDataLogging()
        .EnableDetailedErrors()
        .Options;
}
