namespace EPR.Common.Functions.Test.Database.TestContexts.Entities;

using EPR.Common.Functions.Database.Entities;
using System.Collections.Generic;

internal class Thing : EntityBase
{
    public string Name { get; set; }

    public List<ThingChild> Children { get; set; }
}
