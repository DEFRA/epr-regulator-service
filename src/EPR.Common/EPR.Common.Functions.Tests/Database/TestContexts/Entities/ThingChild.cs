namespace EPR.Common.Functions.Test.Database.TestContexts.Entities;

using Functions.Database.Entities;
using System;

internal class ThingChild : EntityBase
{
    public string Name { get; set; }

    public Guid ThingId { get; set; }

    public Thing Thing { get; set; }
}
