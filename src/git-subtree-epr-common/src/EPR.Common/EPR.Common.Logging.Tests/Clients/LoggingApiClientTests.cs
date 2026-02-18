namespace EPR.Common.Logging.Tests.Clients;

using System.Net;
using Exceptions;
using FluentAssertions;
using Logging.Clients;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;
using Moq;
using Moq.Protected;

[TestClass]
public class LoggingApiClientTests
{
    private Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private ILoggingApiClient? _systemUnderTest;

    [TestInitialize]
    public void Setup()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        var httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        httpClient.BaseAddress = new Uri("https://example.com/");

        _systemUnderTest = new LoggingApiClient(httpClient);
    }

    [TestMethod]
    public async Task LoggingApiClient_SendsEvent_WhenConfigured()
    {
        // Arrange
        LoggingEvent loggingEvent = new(Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, 
            "Unit-Test", "01-01", 0,
            new("UINT_TEST", "unit test", string.Empty), "127.0.0.1");

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        // Act
        var response = await _systemUnderTest.SendEventAsync(loggingEvent);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task LoggingApiClient_WhenSendEventFails_LoggingExceptionIsThrown()
    {
        // Arrange
        LoggingEvent loggingEvent = new(Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, 
            "Unit-Test", "01-01", 0,
            new("UINT_TEST", "unit test", string.Empty), "127.0.0.1");

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.ServiceUnavailable });

        // Act
        Func<Task> act = async () => await _systemUnderTest.SendEventAsync(loggingEvent);

        // Assert
        await act.Should().ThrowAsync<ProtectiveMonitoringLogException>();
    }
}