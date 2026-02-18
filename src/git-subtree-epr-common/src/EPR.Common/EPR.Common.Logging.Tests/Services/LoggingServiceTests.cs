namespace EPR.Common.Logging.Tests.Services;

using System.Net;
using System.Security.Claims;
using FluentAssertions;
using Helpers;
using Logging.Clients;
using Logging.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Identity.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;
using Moq;

[TestClass]
public class LoggingServiceTests
{
    private Mock<ILogger<LoggingService>> _mockLogger;
    private Mock<ILoggingApiClient> _mockLoggingApiClient;
    private IHttpContextAccessor _httpContextAccessor;
    private ILoggingService _systemUnderTest;
    private readonly string _component = "Caller";
    private readonly Guid _sessionId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly string _ip = "1.2.3.4";

    [TestInitialize]
    public void Setup()
    {
        _mockLoggingApiClient = new Mock<ILoggingApiClient>();
        _mockLogger = new Mock<ILogger<LoggingService>>();
    
        var claims = new[] { new Claim( ClaimConstants.ObjectId, _userId.ToString()) };
        var claimsIdentity = new ClaimsIdentity(claims, "CustomAuthenticationType");

        _httpContextAccessor = Mock.Of<IHttpContextAccessor>(); 
        _httpContextAccessor.HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(claimsIdentity) };
        _httpContextAccessor.HttpContext.Request.Headers["HTTP_X_FORWARDED_FOR"] = new StringValues(_ip);
        _httpContextAccessor.HttpContext.Connection.RemoteIpAddress = new IPAddress(1234);

        _systemUnderTest = new LoggingService(_mockLoggingApiClient.Object, _httpContextAccessor, _mockLogger.Object);
    }

    [TestMethod]
    public async Task LoggingService_SendsAntivirusScanResultEvent_WhenFileIsClean()
    {
        // Arrange
        _mockLoggingApiClient.Setup(x => x.SendEventAsync(It.IsAny<LoggingEvent>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
        ProtectiveMonitoringEvent @event = EventGenerator.CreateAntivirusScanResultEvent(
            _component, _sessionId, "pom.csv", true, "FileId: 123456");

        // Act
        var response = await _systemUnderTest.SendEventAsync(@event);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _mockLoggingApiClient.Verify(x => x.SendEventAsync(It.IsAny<LoggingEvent>()), Times.Once);
        _mockLogger.VerifyLog(logger => logger.LogInformation($"Logging event sent by {_component} in session {_sessionId} for user {_userId} from {_ip} at *"));
    }

    [TestMethod]
    public async Task LoggingService_SendsAntivirusScanResultEvent_WhenFileIsCleanAndRemoteIpAddressIsUsed()
    {
        // Arrange
        _httpContextAccessor.HttpContext.Request.Headers["HTTP_X_FORWARDED_FOR"] = new StringValues(string.Empty);
        _mockLoggingApiClient.Setup(x => x.SendEventAsync(It.IsAny<LoggingEvent>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
        ProtectiveMonitoringEvent @event = EventGenerator.CreateAntivirusScanResultEvent(
            _component, _sessionId, "pom.csv", true, "FileId: 123456");
        var expectedIp = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress;

        // Act
        var response = await _systemUnderTest.SendEventAsync(@event);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _mockLoggingApiClient.Verify(x => x.SendEventAsync(It.IsAny<LoggingEvent>()), Times.Once);
        _mockLogger.VerifyLog(logger => logger.LogInformation($"Logging event sent by {_component} in session {_sessionId} for user {_userId} from {expectedIp} at *"));
    }

    [TestMethod]
    public async Task LoggingService_SendAntivirusScanResultEvent_FailsWhenClientThrows()
    {
        // Arrange
        _mockLoggingApiClient.Setup(x => x.SendEventAsync(It.IsAny<LoggingEvent>()))
            .ThrowsAsync(new HttpRequestException());
        ProtectiveMonitoringEvent @event = EventGenerator.CreateAntivirusScanResultEvent(
            _component, _sessionId, "pom.csv", true, "FileId: 123456");

        // Act
        Func<Task> act = async () => await _systemUnderTest.SendEventAsync(@event);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
        _mockLoggingApiClient.Verify(x => x.SendEventAsync(It.IsAny<LoggingEvent>()), Times.Once);
        _mockLogger.VerifyLog(logger => logger.LogError($"Error sending logging event originated by {_component} in session {_sessionId} for user {_userId} from {_ip} at *"));
    }

    [TestMethod]
    public async Task LoggingService_SendsAntivirusScanResultEvent_WhenFileIsInfected()
    {
        // Arrange
        _mockLoggingApiClient.Setup(x => x.SendEventAsync(It.IsAny<LoggingEvent>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
        ProtectiveMonitoringEvent @event = EventGenerator.CreateAntivirusScanResultEvent(
            _component, _sessionId, "pom.csv", false, "FileId: 123456");

        // Act
        var response = await _systemUnderTest.SendEventAsync(@event);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _mockLoggingApiClient.Verify(x => x.SendEventAsync(It.IsAny<LoggingEvent>()), Times.Once);
        _mockLogger.VerifyLog(logger => logger.LogInformation($"Logging event sent by {_component} in session {_sessionId} for user {_userId} from {_ip} at *"));
    }

    [TestMethod]
    public async Task LoggingService_SendsFileUploadedEvent()
    {
        // Arrange
        _mockLoggingApiClient.Setup(x => x.SendEventAsync(It.IsAny<LoggingEvent>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
        ProtectiveMonitoringEvent @event = EventGenerator.CreateFileUploadedEvent(
            _component, _sessionId, "pom.csv", "FileId: 123456");

        // Act
        var response = await _systemUnderTest.SendEventAsync(@event);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _mockLoggingApiClient.Verify(x => x.SendEventAsync(It.IsAny<LoggingEvent>()), Times.Once);
        _mockLogger.VerifyLog(logger => logger.LogInformation($"Logging event sent by {_component} in session {_sessionId} for user {_userId} from {_ip} at *"));
    }

    [TestMethod]
    public async Task LoggingService_SendsFileDownloadedEvent()
    {
        // Arrange
        _mockLoggingApiClient.Setup(x => x.SendEventAsync(It.IsAny<LoggingEvent>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
        ProtectiveMonitoringEvent @event = EventGenerator.CreateFileDownloadedEvent(
            _component, _sessionId, "pom.csv", "FileId: 123456");

        // Act
        var response = await _systemUnderTest.SendEventAsync(@event);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _mockLoggingApiClient.Verify(x => x.SendEventAsync(It.IsAny<LoggingEvent>()), Times.Once);
        _mockLogger.VerifyLog(logger => logger.LogInformation($"Logging event sent by {_component} in session {_sessionId} for user {_userId} from {_ip} at *"));
    }

    [TestMethod]
    public async Task LoggingService_SendsSubmissionCreatedEvent()
    {
        // Arrange
        _mockLoggingApiClient.Setup(x => x.SendEventAsync(It.IsAny<LoggingEvent>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
        ProtectiveMonitoringEvent @event = EventGenerator.CreateSubmissionCreatedEvent(
            _component, _sessionId, Guid.NewGuid(), "FileId: 123456");

        // Act
        var response = await _systemUnderTest.SendEventAsync(@event);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _mockLoggingApiClient.Verify(x => x.SendEventAsync(It.IsAny<LoggingEvent>()), Times.Once);
        _mockLogger.VerifyLog(logger => logger.LogInformation($"Logging event sent by {_component} in session {_sessionId} for user {_userId} from {_ip} at *"));
    }

    [TestMethod]
    public async Task LoggingService_SendsSubmissionUpdatedEvent()
    {
        // Arrange
        _mockLoggingApiClient.Setup(x => x.SendEventAsync(It.IsAny<LoggingEvent>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
        ProtectiveMonitoringEvent @event = EventGenerator.CreateSubmissionUpdatedEvent(
            _component, _sessionId, Guid.NewGuid(), "FileId: 123456");

        // Act
        var response = await _systemUnderTest.SendEventAsync(@event);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _mockLoggingApiClient.Verify(x => x.SendEventAsync(It.IsAny<LoggingEvent>()), Times.Once);
        _mockLogger.VerifyLog(logger => logger.LogInformation($"Logging event sent by {_component} in session {_sessionId} for user {_userId} from {_ip} at *"));
    }

    [TestMethod]
    public async Task LoggingService_SendsSubmissionApprovedEvent()
    {
        // Arrange
        _mockLoggingApiClient.Setup(x => x.SendEventAsync(It.IsAny<LoggingEvent>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
        ProtectiveMonitoringEvent @event = EventGenerator.CreateSubmissionApprovedEvent(
            _component, _sessionId, Guid.NewGuid(), "FileId: 123456");

        // Act
        var response = await _systemUnderTest.SendEventAsync(@event);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _mockLoggingApiClient.Verify(x => x.SendEventAsync(It.IsAny<LoggingEvent>()), Times.Once);
        _mockLogger.VerifyLog(logger => logger.LogInformation($"Logging event sent by {_component} in session {_sessionId} for user {_userId} from {_ip} at *"));
    }

    [TestMethod]
    public async Task LoggingService_SendsSubmissionSubmittedEvent()
    {
        // Arrange
        _mockLoggingApiClient.Setup(x => x.SendEventAsync(It.IsAny<LoggingEvent>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
        ProtectiveMonitoringEvent @event = EventGenerator.CreateSubmissionSubmittedEvent(
            _component, _sessionId, Guid.NewGuid(), "FileId: 123456");

        // Act
        var response = await _systemUnderTest.SendEventAsync(@event);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _mockLoggingApiClient.Verify(x => x.SendEventAsync(It.IsAny<LoggingEvent>()), Times.Once);
        _mockLogger.VerifyLog(logger => logger.LogInformation($"Logging event sent by {_component} in session {_sessionId} for user {_userId} from {_ip} at *"));
    }

    [TestMethod]
    public async Task LoggingService_SendsAcceptanceTermsAndConditionsEvent()
    {
        // Arrange
        _mockLoggingApiClient.Setup(x => x.SendEventAsync(It.IsAny<LoggingEvent>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
        ProtectiveMonitoringEvent @event = EventGenerator.CreateAcceptanceTermsAndConditionsEvent(
            _component, _sessionId, "FileId: 123456");

        // Act
        var response = await _systemUnderTest.SendEventAsync(@event);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _mockLoggingApiClient.Verify(x => x.SendEventAsync(It.IsAny<LoggingEvent>()), Times.Once);
        _mockLogger.VerifyLog(logger => logger.LogInformation($"Logging event sent by {_component} in session {_sessionId} for user {_userId} from {_ip} at *"));
    }
    
    [TestMethod]
    public async Task LoggingService_SendsEvent_WhenUserIdIsProvided()
    {
        // Arrange
        _mockLoggingApiClient.Setup(x => x.SendEventAsync(It.IsAny<LoggingEvent>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
        ProtectiveMonitoringEvent @event = EventGenerator.CreateAntivirusScanResultEvent(
            _component, _sessionId, "pom.csv", true, "FileId: 123456");
        var userId = Guid.NewGuid();

        // Act
        var response = await _systemUnderTest.SendEventAsync(userId, @event);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _mockLoggingApiClient.Verify(x => x.SendEventAsync(It.IsAny<LoggingEvent>()), Times.Once);
        _mockLogger.VerifyLog(logger => logger.LogInformation($"Logging event sent by {_component} in session {_sessionId} for user {userId} from {_ip} at *"));
    }

    [TestMethod]
    public async Task LoggingService_SendsSubmissionEvent_WhenHttpContextIsNull()
    {
        // Arrange
        _mockLoggingApiClient.Setup(x => x.SendEventAsync(It.IsAny<LoggingEvent>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
        ProtectiveMonitoringEvent @event = EventGenerator.CreateSubmissionApprovedEvent(
            _component, _sessionId, Guid.NewGuid(), "FileId: 123456");
        _httpContextAccessor.HttpContext = null;
        var expectedIp = "0.0.0.0";

        // Act
        var response = await _systemUnderTest.SendEventAsync(_userId, @event);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _mockLoggingApiClient.Verify(x => x.SendEventAsync(It.IsAny<LoggingEvent>()), Times.Once);
        _mockLogger.VerifyLog(logger => logger.LogInformation(
            $"Logging event sent by {_component} in session {_sessionId} for user {_userId} from {expectedIp} at *"));
    }
}
