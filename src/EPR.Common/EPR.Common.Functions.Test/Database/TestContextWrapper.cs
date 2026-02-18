namespace EPR.Common.Functions.Test.Database;

using Microsoft.EntityFrameworkCore;
using TestContexts;

internal class TestContextWrapper : BaseTestContextWrapper
{
    public TestContextWrapper()
    {
        this.Context = new TestContext(this.UserContextProvider, this.RequestTimeService, this.EntityDecorators);
        this.Context.Database.OpenConnection();
        this.Context.Database.EnsureCreated();
    }

    public TestContext Context { get; set; }
}
