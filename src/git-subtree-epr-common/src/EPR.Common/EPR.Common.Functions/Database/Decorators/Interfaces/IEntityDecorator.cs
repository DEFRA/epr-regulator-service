namespace EPR.Common.Functions.Database.Decorators.Interfaces;

using Microsoft.EntityFrameworkCore.ChangeTracking;

public interface IEntityDecorator
{
    void BatchStart();

    void Decorate(EntityEntry entityEntry);

    void BatchComplete();
}