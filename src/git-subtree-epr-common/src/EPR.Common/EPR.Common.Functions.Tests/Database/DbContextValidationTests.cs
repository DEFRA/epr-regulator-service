namespace EPR.Common.Functions.Test.Database;

using System.Diagnostics.CodeAnalysis;
using DbObjects;
using FluentAssertions;
using Functions.Database.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestContexts.Entities;
using TestContext = TestContexts.TestContext;

[TestClass]
[SuppressMessage("Performance", "CA1806:Do not ignore method results", Justification = "Constuction of TestContextWrapper calls OpenConnection which throws exceptions under test")]
public class DbContextValidationTests
{
    [TestMethod]
    public void ContextConnecting_WithEntityNotImplementingEntityBase_ShouldFail()
    {
        // Act
        Action action = () => new TestContextWrapper<ThingNotImplementingEntityBase>();

        // Assert
        action.Should().Throw<MissingMethodException>().Which.Message.Should().Contain(
            $"Entity type {nameof(ThingNotImplementingEntityBase)} of entity set {nameof(TestContext.Things)} should derive from {nameof(EntityBase)}.");
    }

    [TestMethod]
    public void ContextConnecting_WithMissingAuditEntityType_ShouldFail()
    {
        // Act
        Action action = () => new TestContextWrapper<Thing>();

        // Assert
        action.Should().Throw<MissingMethodException>().Which.Message.Should()
            .Contain($"Entity set {nameof(TestContext.Things)} has no corresponding audit entity set implementing IAudit<{nameof(Thing)}>");
    }

    [TestMethod]
    public void ContextConnecting_WithMissingEntityType_ShouldFail()
    {
        // Act
        Action action = () => new TestContextWrapper<ThingAudit>();

        // Assert
        action.Should().Throw<MissingMethodException>().Which.Message.Should()
            .Contain($"Audit entity set {nameof(TestContext.Things)} has no corresponding entity set of type {nameof(Thing)}");
    }

    [TestMethod]
    public void ContextConnecting_WithAuditEntityTypeNotImplementingEntityProperty_ShouldFail()
    {
        // Act
        Action action = () => new TestContextWrapper<Thing, ThingAuditNotImplementingName>();

        // Assert
        action.Should().Throw<MissingMethodException>().Which.Message.Should().Contain($"{nameof(ThingAuditNotImplementingName)} should implement _{nameof(Thing.Name)}");
    }
}
