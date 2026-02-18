namespace EPR.Common.Functions.Test.Database.TestContexts;

using Entities;
using Functions.AccessControl.Interfaces;
using Functions.Database.Decorators.Interfaces;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;
using System.Collections.Generic;

internal class TestContext : TestContext<Thing, ThingAudit>
{
    public TestContext(IUserContextProvider userContextProvider, IRequestTimeService requestTimeService, IEnumerable<IEntityDecorator> entityDecorators)
        : base(userContextProvider, requestTimeService, entityDecorators)
    {
    }

    public DbSet<ThingChild> ThingChildren { get; set; }

    public DbSet<ThingChildAudit> ThingChildAudits { get; set; }
}
