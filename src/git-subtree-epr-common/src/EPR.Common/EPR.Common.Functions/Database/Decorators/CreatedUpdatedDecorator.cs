namespace EPR.Common.Functions.Database.Decorators;

using Entities.Interfaces;
using Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Services.Interfaces;

public class CreatedUpdatedDecorator : IEntityDecorator
{
    private readonly IRequestTimeService requestTimeService;

    public CreatedUpdatedDecorator(IRequestTimeService requestTimeService)
    {
        this.requestTimeService = requestTimeService;
    }

    public void BatchStart()
    {
    }

    public void Decorate(EntityEntry entityEntry)
    {
        if (entityEntry.Entity is ICreated created && entityEntry.State == EntityState.Added)
        {
            created.Created = this.requestTimeService.UtcRequest;
        }

        if (entityEntry.Entity is IUpdated updated)
        {
            updated.LastUpdated = this.requestTimeService.UtcRequest;
        }
    }

    public void BatchComplete()
    {
    }
}