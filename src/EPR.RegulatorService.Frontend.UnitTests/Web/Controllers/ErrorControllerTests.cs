using EPR.RegulatorService.Frontend.Web.Controllers.Errors;

using Microsoft.AspNetCore.Http;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers;

using Frontend.Web.Constants;

[TestClass]
public class ErrorControllerTests
{
    private ErrorController _errorController;
    private readonly Mock<ILogger<ErrorController>> _mockLogger = new();

    [TestInitialize]
    public void Setup()
    {
        _errorController = new ErrorController(_mockLogger.Object);
        _errorController.ControllerContext.HttpContext = new DefaultHttpContext();
    }

    [TestMethod]
    public void InvokeError_For404_ReturnsPageNotFound()
    {
        // Arrange
        int statusCode = (int)HttpStatusCode.NotFound;
        string expected = "PageNotFound";
        string backLink = PagePath.Submissions;

        // Act
        var result = _errorController.Error(statusCode, backLink);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<ViewResult>();
        result.ViewName.Should().Be(expected);
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString() == $"Unhandled Application Error: status code {statusCode} backlink {backLink}"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [TestMethod]
    public void InvokeError_For500_ReturnsError()
    {
        // Arrange
        int statusCode = (int)HttpStatusCode.InternalServerError;
        string expected = "Error";
        string backLink = PagePath.Home;

        // Act
        var result = _errorController.Error(statusCode, backLink);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<ViewResult>();
        result.ViewName.Should().Be(expected);
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString() == $"Unhandled Application Error: status code {statusCode} backlink {backLink}"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [TestMethod]
    public void InvokeError_With_Null_Backlink_Should_Not_Fail()
    {
        // Arrange
        int statusCode = (int)HttpStatusCode.InternalServerError;
        string expected = "Error";
        string? backLink = null;

        // Act
        var result = _errorController.Error(statusCode, backLink);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<ViewResult>();
        result.ViewName.Should().Be(expected);
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString() == $"Unhandled Application Error: status code {statusCode} backlink (null)"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [TestMethod]
    public void ServiceNotAvailable_Should_Return_To_CorrectView_With_BackLink()
    {
        // Arrange
        string backLink = PagePath.RegistrationSubmissionDetails;

        // Act
        var result = _errorController.ServiceNotAvailable(backLink);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<ViewResult>();
        result.ViewData.Values.First().Should().Be(backLink);
    }
}