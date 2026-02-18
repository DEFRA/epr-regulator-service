namespace EPR.Common.Functions.Database.Repositories;

using Context.Interfaces;
using Interfaces;

public class DatabaseQueryRepository : IDatabaseQueryRepository
{
    private readonly IEprCommonContext context;

    public DatabaseQueryRepository(IEprCommonContext context)
    {
        this.context = context;
    }
}