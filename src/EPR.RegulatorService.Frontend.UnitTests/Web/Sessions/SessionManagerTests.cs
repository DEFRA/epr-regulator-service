using System.Text;
using System.Text.Json;

using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.UnitTests.TestData;
using EPR.RegulatorService.Frontend.Web.Sessions;

using Microsoft.AspNetCore.Http;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Sessions;

[TestClass]
public class SessionManagerTests
{
    private const string OrganisationName = "TestCo";
    private JourneySession _testSession = new();
    private readonly string _sessionKey = nameof(JourneySession);
    private string _serializedTestSession;
    private byte[] _sessionBytes;
    private Mock<ISession> _sessionMock;
    private JourneySessionManager _sessionManager;
    private int _hashCode;

    [TestInitialize]
    public void Setup()
    {
        var testSubmission = TestSubmission.GetTestSubmission();
        _hashCode = RegulatorSubmissionSession.GetSubmissionHashCode(testSubmission);

        var regulatorSubmissionSession = new RegulatorSubmissionSession();
        regulatorSubmissionSession.OrganisationSubmissions[_hashCode] = testSubmission;

        _testSession = new()
        {
            RegulatorSubmissionSession = regulatorSubmissionSession,
            PermissionManagementSession = new PermissionManagementSession
            {
                Items =
            [
                new PermissionManagementSessionItem { Id = Guid.NewGuid(), Fullname = "Testing User" }
            ]
            }
        };

        _serializedTestSession = JsonSerializer.Serialize(_testSession);
        _sessionBytes = Encoding.UTF8.GetBytes(_serializedTestSession);

        _sessionMock = new Mock<ISession>();
        _sessionManager = new JourneySessionManager();
    }

    [TestMethod]
    public async Task GivenNoSessionInMemory_WhenGetSessionAsyncCalled_ThenSessionReturnedFromSessionStore()
    {
        // Arrange
        _sessionMock.Setup(x => x.TryGetValue(_sessionKey, out _sessionBytes!)).Returns(true);

        // Act
        var session = await _sessionManager.GetSessionAsync(_sessionMock.Object);

        // Assert
        Assert.IsNotNull(session);
        session.Should().BeOfType<JourneySession>();
        Assert.AreEqual(
            expected: session.RegulatorSubmissionSession.OrganisationSubmissions[_hashCode].OrganisationName,
            actual: _testSession.RegulatorSubmissionSession.OrganisationSubmissions[_hashCode].OrganisationName
        );
        _sessionMock.Verify(x => x.LoadAsync(It.IsAny<CancellationToken>()), Times.Once());
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
        Assert.IsNotNull(session);
        session.Should().BeOfType<JourneySession>();
        Assert.AreEqual(
            expected: session.RegulatorSubmissionSession.OrganisationSubmissions[_hashCode].OrganisationName,
            actual: _testSession.RegulatorSubmissionSession.OrganisationSubmissions[_hashCode].OrganisationName
        );
        _sessionMock.Verify(x => x.LoadAsync(It.IsAny<CancellationToken>()), Times.Once);
        _sessionMock.Verify(x => x.TryGetValue(_sessionKey, out It.Ref<byte[]>.IsAny!), Times.Never);
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
        Assert.IsNotNull(savedSession);
        savedSession.Should().BeOfType<JourneySession>();
        Assert.AreEqual(
            expected: savedSession.RegulatorSubmissionSession.OrganisationSubmissions[_hashCode].OrganisationName,
            actual: _testSession.RegulatorSubmissionSession.OrganisationSubmissions[_hashCode].OrganisationName
        );
        _sessionMock.Verify(x => x.LoadAsync(It.IsAny<CancellationToken>()), Times.Once);
        _sessionMock.Verify(x => x.Set(_sessionKey, It.IsAny<byte[]>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenSessionKey_WhenRemoveSessionCalled_ThenSessionRemovedFromMemoryAndSessionStore()
    {
        // Arrange
        _sessionMock.Setup(x => x.Set(_sessionKey, It.IsAny<byte[]>()));

        await _sessionManager.SaveSessionAsync(_sessionMock.Object, _testSession);

        // Act
        _sessionManager.RemoveSession(_sessionMock.Object);
        var savedSession = await _sessionManager.GetSessionAsync(_sessionMock.Object);

        // Assert
        Assert.IsNull(savedSession);
        _sessionMock.Verify(x => x.Remove(_sessionKey), Times.Once);
    }

    [TestMethod]
    public async Task GivenNoSessionInMemory_WhenUpdateSessionAsyncCalled_ThenSessionHasBeenUpdatedInMemoryAndStore()
    {
        // Arrange
        _sessionMock.Setup(x => x.Set(_sessionKey, It.IsAny<byte[]>()));
        await _sessionManager.SaveSessionAsync(_sessionMock.Object, _testSession);

        // Act
        await _sessionManager.UpdateSessionAsync(
            _sessionMock.Object, (x) => x.RegulatorSubmissionSession.OrganisationSubmissions[_hashCode].OrganisationName = OrganisationName
        );
        var savedSession = await _sessionManager.GetSessionAsync(_sessionMock.Object);

        // Assert
        Assert.IsNotNull(savedSession);
        savedSession.Should().BeOfType<JourneySession>();
        Assert.AreEqual(
            expected: OrganisationName,
            actual: savedSession.RegulatorSubmissionSession.OrganisationSubmissions[_hashCode].OrganisationName
        );
        _sessionMock.Verify(x => x.LoadAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        _sessionMock.Verify(x => x.Set(_sessionKey, It.IsAny<byte[]>()), Times.Exactly(2));
    }

    [TestMethod]
    public async Task GivenSessionInMemory_WhenUpdateSessionAsyncCalled_ThenSessionHasBeenUpdatedInMemoryAndStore()
    {
        // Arrange
        _sessionMock.Setup(x => x.Set(_sessionKey, It.IsAny<byte[]>()));
        await _sessionManager.SaveSessionAsync(_sessionMock.Object, _testSession);

        // Act
        await _sessionManager.UpdateSessionAsync(
             _sessionMock.Object, (x) => x.RegulatorSubmissionSession.OrganisationSubmissions[_hashCode].OrganisationName = OrganisationName
        );
        var savedSession = await _sessionManager.GetSessionAsync(_sessionMock.Object);

        // Assert
        Assert.IsNotNull(savedSession);
        savedSession.Should().BeOfType<JourneySession>();
        Assert.AreEqual(
            expected: OrganisationName,
            actual: savedSession.RegulatorSubmissionSession.OrganisationSubmissions[_hashCode].OrganisationName
        );
        _sessionMock.Verify(x => x.LoadAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        _sessionMock.Verify(x => x.Set(_sessionKey, It.IsAny<byte[]>()), Times.Exactly(2));
    }
}
