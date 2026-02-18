namespace EPR.Common.Functions.Test.Database;

using EPR.Common.Functions.Test.Database.TestContexts;
using Microsoft.EntityFrameworkCore;

internal class TestContextWrapper<T1, T2> : BaseTestContextWrapper
    where T1 : class
    where T2 : class
{
    public TestContextWrapper()
    {
        this.Context = new TestContext<T1, T2>(this.UserContextProvider, this.RequestTimeService, this.EntityDecorators);
        this.Context.Database.OpenConnection();
        this.Context.Database.EnsureCreated();
    }

    public TestContext<T1, T2> Context { get; set; }
}
