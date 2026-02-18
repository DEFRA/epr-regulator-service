namespace EPR.Common.Functions.Test.Database;

using FluentAssertions;
using Functions.Database.Entities;
using Functions.Database.Entities.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

[TestClass]
public class CreatedUpdatedTests
{
    [TestMethod]
    public void AddingEntity_ImplementingICreated_ShouldAddCreationInfo()
    {
        // Arrange
        var testContextWrapper = new TestContextWrapper<CreatedThing, CreatedThingAudit>();
        var context = testContextWrapper.Context;

        var creationTime = DateTime.UtcNow;
        testContextWrapper.UtcNow = creationTime;

        // Act
        context.Add(new CreatedThing { Name = "A created thing" });
        context.SaveChanges();

        // Assert
        context.Things.Count().Should().Be(1);
        var createdThing = context.Things.Single();
        createdThing.Name.Should().Be("A created thing");
        createdThing.Created.Should().Be(creationTime);
    }

    [TestMethod]
    public void UpdatingEntity_ImplementingICreated_ShouldKeepOriginalCreationInfo()
    {
        // Arrange
        var testContextWrapper = new TestContextWrapper<CreatedThing, CreatedThingAudit>();
        var context = testContextWrapper.Context;

        var creationTime = DateTime.UtcNow;
        testContextWrapper.UtcNow = creationTime;

        context.Add(new CreatedThing { Name = "A created thing" });
        context.SaveChanges();

        // Act
        testContextWrapper.UtcNow.Returns(creationTime.AddMinutes(10));

        var modifiedThing = context.Things.Single();
        modifiedThing.Name = "Modified created thing";
        context.SaveChanges();

        // Assert
        context.Things.Count().Should().Be(1);
        var createdThing = context.Things.Single();
        createdThing.Name.Should().Be("Modified created thing");
        createdThing.Created.Should().Be(creationTime);
    }

    [TestMethod]
    public void AddingEntity_ImplementingIUpdated_ShouldAddUpdateInfo()
    {
        // Arrange
        var testContextWrapper = new TestContextWrapper<CreatedThing, CreatedThingAudit>();
        var context = testContextWrapper.Context;

        var creationTime = DateTime.UtcNow;
        testContextWrapper.UtcNow = creationTime;

        // Act
        context.Add(new CreatedThing { Name = "A created thing" });
        context.SaveChanges();

        // Assert
        context.Things.Count().Should().Be(1);
        var createdThing = context.Things.Single();
        createdThing.Name.Should().Be("A created thing");
        createdThing.LastUpdated.Should().Be(creationTime);
    }

    [TestMethod]
    public void UpdatingEntity_ImplementingIUpdate_ShouldUpdateUpdateInfo()
    {
        // Act
        var testContextWrapper = new TestContextWrapper<CreatedThing, CreatedThingAudit>();
        var context = testContextWrapper.Context;

        var creationTime = DateTime.UtcNow;
        testContextWrapper.UtcNow = creationTime;

        context.Add(new CreatedThing { Name = "A created thing" });
        context.SaveChanges();

        // Act
        var modifiedTime = creationTime.AddMinutes(10);
        testContextWrapper.UtcNow = modifiedTime;

        var modifiedThing = context.Things.Single();

        modifiedThing.Name = "Modified thing";
        context.SaveChanges();

        // Assert
        context.Things.Count().Should().Be(1);
        var createdThing = context.Things.Single();
        createdThing.Name.Should().Be("Modified thing");
        createdThing.LastUpdated.Should().Be(modifiedTime);
    }

    [TestMethod]
    public void AddingMultipleEntries_ImplementingICreate_ShouldAddCreationInfo()
    {
        // Arrange
        var testContextWrapper = new TestContextWrapper<CreatedThing, CreatedThingAudit>();
        var context = testContextWrapper.Context;

        var creationTime = DateTime.UtcNow;
        testContextWrapper.UtcNow = creationTime;

        // Act
        context.Add(new CreatedThing { Name = "A created thing" });
        context.Add(new CreatedThing { Name = "Another created thing" });
        context.SaveChanges();

        // Assert
        context.Things.Count().Should().Be(2);

        var firstThing = context.Things.Single(thing => thing.Name == "A created thing");
        firstThing.Created.Should().Be(creationTime);

        var secondThing = context.Things.Single(thing => thing.Name == "Another created thing");
        secondThing.Created.Should().Be(creationTime);
    }

    [TestMethod]
    public void AddingMultipleEntries_ImplementingIUpdate_ShouldAddUpdateInfo()
    {
        // Arrange
        var testContextWrapper = new TestContextWrapper<CreatedThing, CreatedThingAudit>();
        var context = testContextWrapper.Context;

        var creationTime = DateTime.UtcNow;
        testContextWrapper.UtcNow = creationTime;

        // Act
        context.Add(new CreatedThing { Name = "A created thing" });
        context.Add(new CreatedThing { Name = "Another created thing" });
        context.SaveChanges();

        // Assert
        context.Things.Count().Should().Be(2);

        var firstThing = context.Things.Single(thing => thing.Name == "A created thing");
        firstThing.LastUpdated.Should().Be(creationTime);

        var secondThing = context.Things.Single(thing => thing.Name == "Another created thing");
        secondThing.LastUpdated.Should().Be(creationTime);
    }

    [TestMethod]
    public void AddingTwoEntriesAndThenUpdatingOne_ImplementingIUpdate_ShouldAlterUpdateInfoOnlyOnTheUpdatedThing()
    {
        // Arrange
        var testContextWrapper = new TestContextWrapper<CreatedThing, CreatedThingAudit>();
        var context = testContextWrapper.Context;

        var creationTime = DateTime.UtcNow;
        testContextWrapper.UtcNow = creationTime;

        context.Add(new CreatedThing { Name = "A created thing" });
        context.Add(new CreatedThing { Name = "Another created thing" });
        context.SaveChanges();

        // Act
        var modifiedTime = creationTime.AddMinutes(10);
        testContextWrapper.UtcNow = modifiedTime;

        var firstThing = context.Things.Single(thing => thing.Name == "A created thing");
        firstThing.Name = "A Modified Thing";
        context.SaveChanges();

        // Assert
        context.Things.Count().Should().Be(2);

        var modifiedThing = context.Things.Single(thing => thing.Name == "A Modified Thing");
        modifiedThing.LastUpdated.Should().Be(modifiedTime);

        var secondThing = context.Things.Single(thing => thing.Name == "Another created thing");
        secondThing.LastUpdated.Should().Be(creationTime);
    }

    [TestMethod]
    public void UpdatingTwoEntriesSeparately_ImplementingIUpdate_ShouldHaveDifferentUpdatedTimeAndSameCreationTime()
    {
        // Arrange
        var testContextWrapper = new TestContextWrapper<CreatedThing, CreatedThingAudit>();
        var context = testContextWrapper.Context;

        var creationTime = DateTime.UtcNow;
        testContextWrapper.UtcNow = creationTime;

        context.Add(new CreatedThing { Name = "A created thing" });
        context.Add(new CreatedThing { Name = "Another created thing" });
        context.SaveChanges();

        // Act
        var firstModifyingTime = creationTime.AddMinutes(10);
        testContextWrapper.UtcNow = firstModifyingTime;

        var firstModifedThing = context.Things.Single(thing => thing.Name == "A created thing");
        firstModifedThing.Name = "Modified first";
        context.SaveChanges();

        var secondModifyingTime = firstModifyingTime.AddMinutes(10);
        testContextWrapper.UtcNow = secondModifyingTime;
        var secondModifedThing = context.Things.Single(thing => thing.Name == "Another created thing");
        secondModifedThing.Name = "Modified second";
        context.SaveChanges();

        // Assert
        var resultFirstThing = context.Things.Single(thing => thing.Name == "Modified first");
        resultFirstThing.LastUpdated.Should().Be(firstModifyingTime);
        resultFirstThing.Created.Should().Be(creationTime);

        var resultSecondThing = context.Things.Single(thing => thing.Name == "Modified second");
        resultSecondThing.LastUpdated.Should().Be(secondModifyingTime);
        resultSecondThing.Created.Should().Be(creationTime);
    }

    [TestMethod]
    public void UpdatingTwoEntriesWithOneUnchanged_ImplementingIUpdate_ShouldUpdateTheChangedEntityUpdateColumnsOnly()
    {
        // Arrange
        var testContextWrapper = new TestContextWrapper<CreatedThing, CreatedThingAudit>();
        var context = testContextWrapper.Context;

        var creationTime = DateTime.UtcNow;
        testContextWrapper.UtcNow = creationTime;

        context.Add(new CreatedThing { Name = "A created thing" });
        context.Add(new CreatedThing { Name = "Another created thing" });
        context.SaveChanges();

        // Act
        var firstModifyingTime = creationTime.AddMinutes(10);
        testContextWrapper.UtcNow = firstModifyingTime;
        var firstModifiedThing = context.Things.Single(thing => thing.Name == "A created thing");
        firstModifiedThing.Name = "Modified first";
        context.SaveChanges();

        var secondModifyingTime = firstModifyingTime.AddMinutes(10);
        testContextWrapper.UtcNow.Returns(secondModifyingTime);

        var secondUnmodifiedThing = context.Things.Single(thing => thing.Name == "Another created thing");
        context.SaveChanges();

        // Assert
        var resultFirstThing = context.Things.Single(thing => thing.Name == "Modified first");
        resultFirstThing.LastUpdated.Should().Be(firstModifyingTime);
        resultFirstThing.Created.Should().Be(creationTime);

        var resultSecondThing = context.Things.Single(thing => thing.Name == "Another created thing");
        resultSecondThing.LastUpdated.Should().Be(creationTime);
        resultSecondThing.Created.Should().Be(creationTime);
    }

    private class CreatedThing : EntityBase, ICreated, IUpdated
    {
        public string Name { get; set; }

        public DateTime Created { get; set; }

        public DateTime LastUpdated { get; set; }
    }

#pragma warning disable SA1300 // Element should begin with upper-case letter
    private class CreatedThingAudit : AuditEntityBase, IAudit<CreatedThing>
    {
        public string _Name { get; set; }

        public DateTime _Created { get; set; }

        public DateTime _LastUpdated { get; set; }
    }
}
