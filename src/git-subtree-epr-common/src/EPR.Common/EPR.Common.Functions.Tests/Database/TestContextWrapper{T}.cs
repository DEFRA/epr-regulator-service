namespace EPR.Common.Functions.Test.Database;

using EPR.Common.Functions.Test.Database.TestContexts;
using Microsoft.EntityFrameworkCore;

internal class TestContextWrapper<T> : BaseTestContextWrapper
    where T : class
{
    public TestContextWrapper()
    {
        this.Context = new TestContext<T>(this.RequestTimeService, this.UserContextProvider, this.EntityDecorators);
        this.Context.Database.OpenConnection();
        this.Context.Database.EnsureCreated();
    }

    public TestContext<T> Context { get; set; }
}
