namespace EPR.Common.Functions.Test.Database.TestContexts.Entities;

using Functions.Database.Entities;
using Functions.Database.Entities.Interfaces;
using System;

internal class ThingChildAudit : AuditEntityBase, IAudit<ThingChild>
{
#pragma warning disable SA1300 // Element should begin with upper-case letter
    public string _Name { get; set; }

    public Guid _ThingId { get; set; }
}
