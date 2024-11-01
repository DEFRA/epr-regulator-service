using System.Net;

using EPR.RegulatorService.Frontend.Web.Controllers.Errors;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers;

using Frontend.Web.Constants;

[TestClass]
public class ErrorControllerTests
{
    private ErrorController _errorController;

    [TestInitialize]
    public void Setup()
    {
        _errorController = new ErrorController();
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
    }
}