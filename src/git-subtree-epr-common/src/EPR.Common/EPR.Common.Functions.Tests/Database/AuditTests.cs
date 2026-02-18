namespace EPR.Common.Functions.Test.Database;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestContexts.Entities;

[TestClass]
public class AuditTests
{
    [TestMethod]
    public async Task CreateThing_ShouldCreateThingAuditWithAddedState()
    {
        // Arrange
        var testContextWrapper = new TestContextWrapper();
        var context = testContextWrapper.Context;

        var dateTimeNow = DateTime.UtcNow;

        var thing = new Thing
        {
            Name = nameof(Thing.Name),
        };

        // Act
        context.Things.Add(thing);
        await context.SaveChangesAsync();

        // Assert
        var thingAudit = await context.ThingAudits.SingleOrDefaultAsync(x => x._Id == thing.Id && (EntityState)x.EntityState == EntityState.Added);
        thingAudit.Should().NotBeNull();
        thingAudit._Name.Should().Be(nameof(Thing.Name));
        AssertAuditFields(thingAudit, testContextWrapper);
    }

    [TestMethod]
    public async Task EditThing_ShouldCreateThingAuditWithModifiedState()
    {
        // Arrange
        var testContextWrapper = new TestContextWrapper();
        var context = testContextWrapper.Context;
        var thing = new Thing
        {
            Name = nameof(Thing.Name),
        };

        context.Things.Add(thing);
        await context.SaveChangesAsync();

        // Act
        const string thingName = "Modified";
        thing.Name = thingName;
        await context.SaveChangesAsync();

        // Assert
        var thingAudit = await context.ThingAudits.SingleOrDefaultAsync(x => x._Id == thing.Id && (EntityState)x.EntityState == EntityState.Modified);
        thingAudit.Should().NotBeNull();
        thingAudit._Name.Should().Be(thingName);
        AssertAuditFields(thingAudit, testContextWrapper);
    }

    [TestMethod]
    public async Task DeleteThing_ShouldCreateThingAuditWithModifiedState()
    {
        // Arrange
        var testContextWrapper = new TestContextWrapper();
        var context = testContextWrapper.Context;

        var thing = new Thing
        {
            Name = nameof(Thing.Name),
        };

        context.Things.Add(thing);
        await context.SaveChangesAsync();

        // Act
        context.Things.Remove(thing);
        await context.SaveChangesAsync();

        // Assert
        var thingAudit = await context.ThingAudits.SingleOrDefaultAsync(x => x._Id == thing.Id && (EntityState)x.EntityState == EntityState.Deleted);
        thingAudit.Should().NotBeNull();
        AssertAuditFields(thingAudit, testContextWrapper);
    }

    private static void AssertAuditFields(ThingAudit thingAudit, TestContextWrapper testContextWrapper)
    {
        thingAudit.AuditCreated.Should().Be(testContextWrapper.UtcNow);
    }
}
