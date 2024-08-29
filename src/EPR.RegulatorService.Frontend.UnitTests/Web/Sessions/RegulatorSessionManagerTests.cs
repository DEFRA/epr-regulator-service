using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Sessions;
using Microsoft.AspNetCore.Http;

using System.Text;
using System.Text.Json;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Sessions;

[TestClass]
public class RegulatorSessionManagerTests
{
    private const string OrganisationName = "TestCo";
    private readonly Guid _organisationId = Guid.NewGuid();

    private readonly RegulatorSession _testSession = new();
    private readonly string _sessionKey = nameof(RegulatorSession);

    private string _serializedTestSession;
    private byte[] _sessionBytes;

    private Mock<ISession> _sessionMock;
    private ISessionManager<RegulatorSession> _sessionManager;

    [TestInitialize]
    public void Setup()
    {
        _testSession.OrganisationId = _organisationId;
        _testSession.OrganisationName = OrganisationName;

        _serializedTestSession = JsonSerializer.Serialize(_testSession);
        _sessionBytes = Encoding.UTF8.GetBytes(_serializedTestSession);

        _sessionMock = new Mock<ISession>();
        _sessionManager = new RegulatorSessionManager();
    }

    [TestMethod]
    public async Task GivenNoSessionInMemory_WhenGetSessionAsyncCalled_ThenSessionReturnedFromSessionStore()
    {
        // Arrange        
        _sessionMock.Setup(x => x.TryGetValue(_sessionKey, out _sessionBytes!)).Returns(true);

        // Act
        var session = await _sessionManager.GetSessionAsync(_sessionMock.Object);

        // Assert
        _sessionMock.Verify(x => x.LoadAsync(It.IsAny<CancellationToken>()), Times.Once());
        Assert.IsNotNull(session);
        session.Should().BeOfType<RegulatorSession>();
        Assert.AreEqual(expected: _organisationId, actual: session.OrganisationId);
        Assert.AreEqual(expected: OrganisationName, actual: session.OrganisationName);
    }

    [TestMethod]
    public async Task GivenSessionInMemory_WhenGetSessionAsyncCalled_ThenSessionReturnedFromMemory()
    {
        // Arrange
        _sessionMock.Setup(x => x.Set(_sessionKey, It.IsAny<byte[]>()));
        await _sessionManager.SaveSessionAsync(_sessionMock.Object, _testSession);

        // Act
        var session = await _sessionManager.GetSessionAsync(_sessionMock.Object);

        // Assert
        _sessionMock.Verify(x => x.LoadAsync(It.IsAny<CancellationToken>()), Times.Once);
        _sessionMock.Verify(x => x.TryGetValue(_sessionKey, out It.Ref<byte[]>.IsAny!), Times.Never);
        Assert.IsNotNull(session);
        session.Should().BeOfType<RegulatorSession>();
        Assert.AreEqual(expected: _organisationId, actual: session.OrganisationId);
        Assert.AreEqual(expected: OrganisationName, actual: session.OrganisationName);
    }

    [TestMethod]
    public async Task GivenNewSession_WhenSaveSessionAsyncCalled_ThenSessionSavedInStoreAndMemory()
    {
        // Arrange
        _sessionMock.Setup(x => x.Set(_sessionKey, It.IsAny<byte[]>()));

        // Act
        await _sessionManager.SaveSessionAsync(_sessionMock.Object, _testSession);
        var savedSession = await _sessionManager.GetSessionAsync(_sessionMock.Object);

        // Assert
        _sessionMock.Verify(x => x.LoadAsync(It.IsAny<CancellationToken>()), Times.Once);
        _sessionMock.Verify(x => x.Set(_sessionKey, It.IsAny<byte[]>()), Times.Once);
        Assert.IsNotNull(savedSession);
        savedSession.Should().BeOfType<RegulatorSession>();
        Assert.AreEqual(expected: _organisationId, actual: savedSession.OrganisationId);
        Assert.AreEqual(expected: OrganisationName, actual: savedSession.OrganisationName);
    }

    [TestMethod]
    public async Task GivenSessionKey_WhenRemoveSessionCalled_ThenSessionRemovedFromMemoryAndSessionStore()
    {
        // Arrange
        _sessionMock.Setup(x => x.Set(_sessionKey, It.IsAny<byte[]>()));

        // Act
        await _sessionManager.SaveSessionAsync(_sessionMock.Object, _testSession);
        _sessionManager.RemoveSession(_sessionMock.Object);

        // Assert
        var savedSession = await _sessionManager.GetSessionAsync(_sessionMock.Object);
        _sessionMock.Verify(x => x.Remove(_sessionKey), Times.Once);

        savedSession.Should().BeNull();
    }

    [TestMethod]
    public async Task GivenNoSessionInMemory_WhenUpdateSessionAsyncCalled_ThenSessionHasBeenUpdatedInMemoryAndStore()
    {
        // Arrange
        string testName = "Testing Company";
        Guid testId = Guid.NewGuid();

        // Act        
        await _sessionManager.UpdateSessionAsync(_sessionMock.Object, (x) => x.OrganisationId = testId);
        await _sessionManager.UpdateSessionAsync(_sessionMock.Object, (x) => x.OrganisationName = testName);
        var savedSession = await _sessionManager.GetSessionAsync(_sessionMock.Object);

        // Assert
        Assert.IsNotNull(savedSession);
        Assert.AreEqual(expected: testId, actual: savedSession.OrganisationId);
        Assert.AreEqual(expected: testName, actual: savedSession.OrganisationName);
    }

    [TestMethod]
    public async Task GivenSessionInMemory_WhenUpdateSessionAsyncCalled_ThenSessionHasBeenUpdatedInMemoryAndStore()
    {
        // Arrange
        string testName = "Testing Company";
        Guid testId = Guid.NewGuid();

        _sessionMock.Setup(x => x.Set(_sessionKey, It.IsAny<byte[]>()));
        await _sessionManager.SaveSessionAsync(_sessionMock.Object, _testSession);

        // Act
        await _sessionManager.UpdateSessionAsync(_sessionMock.Object, (x) => x.OrganisationId = testId);
        await _sessionManager.UpdateSessionAsync(_sessionMock.Object, (x) => x.OrganisationName = testName);
        var savedSession = await _sessionManager.GetSessionAsync(_sessionMock.Object);

        // Assert
        Assert.IsNotNull(savedSession);
        Assert.AreEqual(expected: testId, actual: savedSession.OrganisationId);
        Assert.AreEqual(expected: testName, actual: savedSession.OrganisationName);
    }
}