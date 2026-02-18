namespace EPR.Common.Functions.Test.Database.DbObjects;

using Functions.Database.Entities;
using Functions.Database.Entities.Interfaces;
using TestContexts.Entities;

internal class ThingNotImplementingEntityBase
{
    public int Id { get; set; }
}

internal class ThingAuditNotImplementingName : EntityBase, IAudit<Thing>
{
    public int EntityState { get; set; }

    public DateTime AuditCreated { get; set; }
}

internal class ThingAuditImplementingGnome : EntityBase, IAudit<Thing>
{
#pragma warning disable SA1300 // Element should begin with upper-case letter
    public string _Name { get; set; }

    public string _Gnome { get; set; }
#pragma warning restore SA1300 // Element should begin with upper-case letter

    public int EntityState { get; set; }

    public DateTime AuditCreated { get; set; }
}