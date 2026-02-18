namespace EPR.Common.Functions.Database.Entities.Interfaces;

public interface IAudit<TEntity> : IAudit
{
    // n.b. generic here is used by convention to reference the audited entity type
}

public interface IAudit
{
    public int EntityState { get; set; }

    public DateTime AuditCreated { get; set; }
}