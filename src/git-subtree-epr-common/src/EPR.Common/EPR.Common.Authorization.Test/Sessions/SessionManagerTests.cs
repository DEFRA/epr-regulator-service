namespace EPR.Common.Authorization.Test.Sessions;

using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using Authorization.Sessions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using TestClasses;

[TestClass]
public class SessionManagerTests
{
    private const string Name = "TestCo";
    private readonly TestSessionData _testSession = new() { Name = Name };
    private const string SessionKey = nameof(TestSessionData);

    private string _serializedTestSession;
    private byte[] _sessionBytes;

    private Mock<ISession> _sessionMock;
    private ISessionManager<TestSessionData> _sessionManager;

    [TestInitialize]
    public void Setup()
    {
        _serializedTestSession = JsonSerializer.Serialize(_testSession);
        _sessionBytes = Encoding.UTF8.GetBytes(_serializedTestSession);

        _sessionMock = new Mock<ISession>();
        _sessionManager = new SessionManager<TestSessionData>();
    }

    [TestMethod]
    public async Task GivenNoSessionInMemory_WhenGetSessionAsyncCalled_ThenSessionReturnedFromSessionStore()
    {
        // Arrange
        _sessionMock.Setup(x => x.TryGetValue(SessionKey, out _sessionBytes)).Returns(true);

        // Act
        var session = await _sessionManager.GetSessionAsync(_sessionMock.Object);

        // Assert
        _sessionMock.Verify(x => x.LoadAsync(It.IsAny<CancellationToken>()), Times.Once());

        session.Name.Should().Be(_testSession.Name);
    }

    [TestMethod]
    public async Task GivenSessionInMemory_WhenGetSessionAsyncCalled_ThenSessionReturnedFromMemory()
    {
        // Arrange
        _sessionMock.Setup(x => x.Set(SessionKey, It.IsAny<byte[]>()));
        await _sessionManager.SaveSessionAsync(_sessionMock.Object, _testSession);

        // Act
        var session = await _sessionManager.GetSessionAsync(_sessionMock.Object);

        // Assert
        _sessionMock.Verify(x => x.LoadAsync(It.IsAny<CancellationToken>()), Times.Once);
        _sessionMock.Verify(x => x.TryGetValue(SessionKey, out It.Ref<byte[]>.IsAny), Times.Never);

        session.Name.Should().Be(_testSession.Name);
    }

    [TestMethod]
    public async Task GivenNewSession_WhenSaveSessionAsyncCalled_ThenSessionSavedInStoreAndMemory()
    {
        // Arrange
        _sessionMock.Setup(x => x.Set(SessionKey, It.IsAny<byte[]>()));

        // Act
        await _sessionManager.SaveSessionAsync(_sessionMock.Object, _testSession);

        // Assert
        var savedSession = await _sessionManager.GetSessionAsync(_sessionMock.Object);

        _sessionMock.Verify(x => x.LoadAsync(It.IsAny<CancellationToken>()), Times.Once);
        _sessionMock.Verify(x => x.Set(SessionKey, It.IsAny<byte[]>()), Times.Once);

        savedSession.Should().NotBeNull();
        savedSession.Name.Should().Be(_testSession.Name);
    }

    [TestMethod]
    public async Task GivenSessionKey_WhenRemoveSessionCalled_ThenSessionRemovedFromMemoryAndSessionStore()
    {
        // Arrange
        _sessionMock.Setup(x => x.Set(SessionKey, It.IsAny<byte[]>()));

        await _sessionManager.SaveSessionAsync(_sessionMock.Object, _testSession);

        // Act
        _sessionManager.RemoveSession(_sessionMock.Object);

        // Assert
        var savedSession = await _sessionManager.GetSessionAsync(_sessionMock.Object);

        _sessionMock.Verify(x => x.Remove(SessionKey), Times.Once);

        savedSession.Should().BeNull();
    }

    [TestMethod]
    public async Task GivenNoSessionInMemory_WhenUpdateSessionAsyncCalled_ThenSessionHasBeenUpdatedInMemoryAndStore()
    {
        // Act
        await _sessionManager.UpdateSessionAsync(_sessionMock.Object, (x) => x.Name = Name);

        // Assert
        var savedSession = await _sessionManager.GetSessionAsync(_sessionMock.Object);

        _sessionMock.Verify(x => x.LoadAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        _sessionMock.Verify(x => x.Set(SessionKey, It.IsAny<byte[]>()), Times.Once);

        savedSession.Should().NotBeNull();
        savedSession.Name.Should().Be(Name);
    }

    [TestMethod]
    public async Task GivenSessionInMemory_WhenUpdateSessionAsyncCalled_ThenSessionHasBeenUpdatedInMemoryAndStore()
    {
        _sessionMock.Setup(x => x.Set(SessionKey, It.IsAny<byte[]>()));
        await _sessionManager.SaveSessionAsync(_sessionMock.Object, _testSession);

        // Act
        await _sessionManager.UpdateSessionAsync(_sessionMock.Object, (x) => x.Name = Name);

        // Assert
        var savedSession = await _sessionManager.GetSessionAsync(_sessionMock.Object);

        _sessionMock.Verify(x => x.LoadAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        _sessionMock.Verify(x => x.Set(SessionKey, It.IsAny<byte[]>()), Times.Exactly(2));

        savedSession.Should().NotBeNull();
        savedSession.Name.Should().Be(Name);
    }
}