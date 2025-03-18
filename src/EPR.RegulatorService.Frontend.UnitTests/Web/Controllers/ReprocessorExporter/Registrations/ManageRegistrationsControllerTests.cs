using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter;

using FluentAssertions.Execution;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers.ReprocessorExporter.Registrations;

[TestClass]
public class ManageRegistrationsControllerTests
{
    private const string BackLinkViewDataKey = "BackLinkToDisplay";

    private ManageRegistrationsController _controller;
    private Mock<IRegistrationService> _registrationServiceMock;
    private Mock<ILogger<ManageRegistrationsController>> _loggerMock;

    [TestInitialize]
    public void TestInitialize()
    {
        _registrationServiceMock = new Mock<IRegistrationService>();
        _loggerMock = new Mock<ILogger<ManageRegistrationsController>>();

        _controller = new ManageRegistrationsController(_registrationServiceMock.Object, _loggerMock.Object);
    }

    [TestMethod]
    public void Index_ShouldDisplayBackLink()
    {
        // Arrange
        var id = 1;
        var registration = new RegistrationDto
        {
            Id = id,
            OrganisationName = "Test Exporter Ltd",
            SiteAddress = "N/A",
            OrganisationType = ApplicationOrganisationType.Exporter,
            Regulator = "Environment Agency (EA)"
        };

        _registrationServiceMock.Setup(s => s.GetRegistrationById(id)).Returns(registration);

        // Act
        var result = _controller.Index(id);

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        using (new AssertionScope())
        {
            viewResult.ViewData.Should().ContainKey(BackLinkViewDataKey);
            viewResult.ViewData[BackLinkViewDataKey].Should().NotBeNull();
        }
    }

    [TestMethod]
    public void Index_ValidId_ShouldReturnViewResultWithCorrectModel()
    {
        // Arrange
        var id = 1;
        var registration = new RegistrationDto
        {
            Id = id,
            OrganisationName = "Test Exporter Ltd",
            SiteAddress = "N/A",
            OrganisationType = ApplicationOrganisationType.Exporter,
            Regulator = "Environment Agency (EA)"
        };

        _registrationServiceMock.Setup(s => s.GetRegistrationById(id)).Returns(registration);

        // Act
        var result = _controller.Index(id);

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();

        using (new AssertionScope())
        {
            viewResult!.ViewData.Keys.Should().Contain(BackLinkViewDataKey);
            viewResult.Model.Should().BeOfType<ManageRegistrationsViewModel>();

            var model = viewResult.Model as ManageRegistrationsViewModel;
            model.Should().NotBeNull();
            model!.Id.Should().Be(id);
            model.OrganisationName.Should().Be(registration.OrganisationName);
            model.SiteAddress.Should().Be(registration.SiteAddress);
            model.ApplicationOrganisationType.Should().Be(ApplicationOrganisationType.Exporter);
            model.Regulator.Should().Be(registration.Regulator);
        }
    }

    [TestMethod]
    public void Index_InvalidId_ShouldReturnBadRequest()
    {
        // Arrange
        var id = 0; // Invalid ID

        // Act
        var result = _controller.Index(id);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            badRequestResult.Should().NotBeNull();
            badRequestResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            badRequestResult.Value.Should().Be("Invalid registration ID.");
        }

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid ID received")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [TestMethod]
    public void Index_NonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var id = 9999;
        _registrationServiceMock.Setup(s => s.GetRegistrationById(id)).Returns((RegistrationDto)null);

        // Act
        var result = _controller.Index(id);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;

        using (new AssertionScope())
        {
            notFoundResult.Should().NotBeNull();
            notFoundResult!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be("Registration not found.");
        }

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No registration found")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [TestMethod]
    public void Index_Exception_ShouldRedirectToErrorPage()
    {
        // Arrange
        var id = 5;
        _registrationServiceMock.Setup(s => s.GetRegistrationById(id)).Throws(new Exception("Test exception"));

        // Act
        var result = _controller.Index(id);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        var redirectResult = result as RedirectToActionResult;
        using (new AssertionScope())
        {
            redirectResult.Should().NotBeNull();
            redirectResult!.ActionName.Should().Be(PagePath.Error);
            redirectResult.ControllerName.Should().Be("Error");
        }

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Exception occurred while processing ManageRegistrations request")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}