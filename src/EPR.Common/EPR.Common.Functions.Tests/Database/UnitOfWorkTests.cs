namespace EPR.Common.Functions.Test.Database;

using FluentAssertions;
using Functions.CancellationTokens.Interfaces;
using Functions.Database.Context.Interfaces;
using Functions.Database.UnitOfWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using TestContexts.Entities;

[TestClass]
public class UnitOfWorkTests
{
    private readonly Thing testThing = new ()
    {
        Name = nameof(Thing),
        Children = new List<ThingChild>(),
    };

    [TestMethod]
    public void ExecuteT_WithValidImplementation_InvokesImplementationSavesChangesAndReturnsResult()
    {
        // Arrange
        var context = Substitute.For<IEprCommonContext>();

        // Act
        var result = new UnitOfWork(context, Substitute.For<ICancellationTokenAccessor>()).Execute(() => this.testThing);

        // Assert
        result.Should().BeEquivalentTo(this.testThing);
        context.Received(1).SaveChanges();
    }

    [TestMethod]
    public void ExecuteT_WithValidImplementationAndSaveOnException_InvokesImplementationSavesChangesAndReturnsResult()
    {
        // Arrange
        var context = Substitute.For<IEprCommonContext>();

        // Act
        var result = new UnitOfWork(context, Substitute.For<ICancellationTokenAccessor>()).Execute(() => this.testThing, true);

        // Assert
        result.Should().BeEquivalentTo(this.testThing);
        context.Received(1).SaveChanges();
    }

    [TestMethod]
    public void ExecuteT_WithInvalidImplementation_ThrowsException()
    {
        // Arrange
        var context = Substitute.For<IEprCommonContext>();

        // Act
        var act = () => new UnitOfWork(context, Substitute.For<ICancellationTokenAccessor>()).Execute<Thing>(() => throw new Exception());

        // Assert
        act.Should().ThrowExactly<Exception>();
        context.DidNotReceive().SaveChanges();
    }

    [TestMethod]
    public void ExecuteT_WithInvalidImplementationAndSaveOnException_SavesChangesOnException()
    {
        // Arrange
        var context = Substitute.For<IEprCommonContext>();

        // Act
        var act = () => new UnitOfWork(context, Substitute.For<ICancellationTokenAccessor>()).Execute<Thing>(() => throw new Exception(), true);

        // Assert
        act.Should().ThrowExactly<Exception>();
        context.Received(1).SaveChanges();
    }

    [TestMethod]
    public async Task ExecuteAsyncT_WithValidImplementation_InvokesImplementationSavesChangesAndReturnsResult()
    {
        // Arrange
        var context = Substitute.For<IEprCommonContext>();

        // Act
        var result = await new UnitOfWork(context, Substitute.For<ICancellationTokenAccessor>()).ExecuteAsync(() => Task.FromResult(this.testThing));

        // Assert
        result.Should().BeEquivalentTo(this.testThing);
        context.Received(1).SaveChangesAsync();
    }

    [TestMethod]
    public async Task ExecuteAsyncT_WithValidImplementationAndSaveOnException_InvokesImplementationSavesChangesAndReturnsResult()
    {
        // Arrange
        var context = Substitute.For<IEprCommonContext>();

        // Act
        var result = await new UnitOfWork(context, Substitute.For<ICancellationTokenAccessor>()).ExecuteAsync(() => Task.FromResult(this.testThing), true);

        // Assert
        result.Should().BeEquivalentTo(this.testThing);
        context.Received(1).SaveChangesAsync();
    }

    [TestMethod]
    public async Task ExecuteAsyncT_WithInvalidImplementation_ThrowsException()
    {
        // Arrange
        var context = Substitute.For<IEprCommonContext>();

        // Act
        var act = async () => await new UnitOfWork(context, Substitute.For<ICancellationTokenAccessor>()).ExecuteAsync<Thing>(() => throw new Exception());

        // Assert
        act.Should().ThrowExactlyAsync<Exception>();
        context.DidNotReceive().SaveChangesAsync();
    }

    [TestMethod]
    public void ExecuteAsyncT_WithInvalidImplementationAndSaveOnException_SavesChangesOnException()
    {
        // Arrange
        var context = Substitute.For<IEprCommonContext>();

        // Act
        var act = async () => await new UnitOfWork(context, Substitute.For<ICancellationTokenAccessor>()).ExecuteAsync<Thing>(() => throw new Exception(), true);

        // Assert
        act.Should().ThrowExactlyAsync<Exception>();
        context.Received(1).SaveChangesAsync();
    }

    [TestMethod]
    public void Execute_WithValidImplementation_InvokesImplementationSavesChangesAndReturns1()
    {
        // Arrange
        var context = Substitute.For<IEprCommonContext>();
        context.SaveChanges().Returns(1);

        // Act
        var result = new UnitOfWork(context, Substitute.For<ICancellationTokenAccessor>()).Execute(() => { });

        // Assert
        result.Should().Be(1);
        context.Received(1).SaveChanges();
    }

    [TestMethod]
    public void Execute_WithValidImplementationAndSaveOnException_InvokesImplementationSavesChangesReturns1()
    {
        // Arrange
        var context = Substitute.For<IEprCommonContext>();
        context.SaveChanges().Returns(1);

        // Act
        var result = new UnitOfWork(context, Substitute.For<ICancellationTokenAccessor>()).Execute(() => { }, true);

        // Assert
        result.Should().Be(1);
        context.Received(1).SaveChanges();
    }

    [TestMethod]
    public void Execute_WithInvalidImplementation_ThrowsException()
    {
        // Arrange
        var context = Substitute.For<IEprCommonContext>();

        // Act
        var act = () => new UnitOfWork(context, Substitute.For<ICancellationTokenAccessor>()).Execute(() => throw new Exception());

        // Assert
        act.Should().ThrowExactly<Exception>();
        context.DidNotReceive().SaveChanges();
    }

    [TestMethod]
    public void Execute_WithInvalidImplementationAndSaveOnException_SavesChangesOnException()
    {
        // Arrange
        var context = Substitute.For<IEprCommonContext>();

        // Act
        var act = () => new UnitOfWork(context, Substitute.For<ICancellationTokenAccessor>()).Execute(() => throw new Exception(), true);

        // Assert
        act.Should().ThrowExactly<Exception>();
        context.Received(1).SaveChanges();
    }

    [TestMethod]
    public async Task ExecuteAsync_WithValidImplementation_InvokesImplementationSavesChangesAndReturnsResult()
    {
        // Arrange
        var context = Substitute.For<IEprCommonContext>();
        context.SaveChangesAsync().Returns(1);

        // Act
        var result = await new UnitOfWork(context, Substitute.For<ICancellationTokenAccessor>()).ExecuteAsync(() => Task.CompletedTask);

        // Assert
        result.Should().Be(1);
        context.Received(1).SaveChangesAsync();
    }

    [TestMethod]
    public async Task ExecuteAsync_WithValidImplementationAndSaveOnException_InvokesImplementationSavesChangesAndReturnsResult()
    {
        // Arrange
        var context = Substitute.For<IEprCommonContext>();
        context.SaveChangesAsync().Returns(1);

        // Act
        var result = await new UnitOfWork(context, Substitute.For<ICancellationTokenAccessor>()).ExecuteAsync(() => Task.CompletedTask, true);

        // Assert
        result.Should().Be(1);
        context.Received(1).SaveChangesAsync();
    }

    [TestMethod]
    public void ExecuteAsync_WithInvalidImplementation_ThrowsException()
    {
        // Arrange
        var context = Substitute.For<IEprCommonContext>();

        // Act
        var act = () => new UnitOfWork(context, Substitute.For<ICancellationTokenAccessor>()).ExecuteAsync(() => throw new Exception());

        // Assert
        act.Should().ThrowExactlyAsync<Exception>();
        context.DidNotReceive().SaveChangesAsync();
    }

    [TestMethod]
    public void ExecuteAsync_WithInvalidImplementationAndSaveOnException_SavesChangesOnException()
    {
        // Arrange
        var context = Substitute.For<IEprCommonContext>();

        // Act
        var act = () => new UnitOfWork(context, Substitute.For<ICancellationTokenAccessor>()).ExecuteAsync(() => throw new Exception(), true);

        // Assert
        act.Should().ThrowExactlyAsync<Exception>();
        context.Received(1).SaveChangesAsync();
    }

    [TestMethod]
    public void TryAppLock_Succeeds()
    {
        // Arrange
        var wrapper = new TestContextWrapper();

        // Act
        var act = () => new UnitOfWork(wrapper.Context, Substitute.For<ICancellationTokenAccessor>()).TryAppLock(Guid.NewGuid());

        // Assert
        act.Should().NotThrow();
    }

    [TestMethod]
    public async Task TryAppLockAsync_Succeeds()
    {
        // Arrange
        var wrapper = new TestContextWrapper();

        // Act
        var act = () => new UnitOfWork(wrapper.Context, Substitute.For<ICancellationTokenAccessor>()).TryAppLockAsync(Guid.NewGuid());

        // Assert
        act.Should().NotThrowAsync();
    }
}
