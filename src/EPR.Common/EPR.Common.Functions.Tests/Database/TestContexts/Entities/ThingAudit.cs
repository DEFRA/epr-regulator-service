namespace EPR.Common.Functions.Test.Database.TestContexts.Entities;

using Functions.Database.Entities;
using Functions.Database.Entities.Interfaces;

internal class ThingAudit : AuditEntityBase, IAudit<Thing>
{
#pragma warning disable SA1300 // Element should begin with upper-case letter
    public string _Name { get; set; }

    public List<ThingChild> _Children { get; set; }
}
