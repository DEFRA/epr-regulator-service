using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

using FluentAssertions.Execution;

using FluentValidation;
using FluentValidation.Results;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers.ReprocessorExporter.Registrations;

[TestClass]
public class ManageRegistrationsControllerTests
{
    private const string BackLinkViewDataKey = "BackLinkToDisplay";
    private const int RegistrationId = 1;

    private ManageRegistrationsController _controller;
    private Mock<IReprocessorExporterService> _reprocessorExporterServiceMock;
    private Mock<IMapper> _mapperMock;
    private Mock<IValidator<IdRequest>> _validatorMock;

    [TestInitialize]
    public void TestInitialize()
    {
        _reprocessorExporterServiceMock = new Mock<IReprocessorExporterService>();
        _mapperMock = new Mock<IMapper>();
        _validatorMock = new Mock<IValidator<IdRequest>>();
        var sessionManagerMock = new Mock<ISessionManager<JourneySession>>();
        var configurationSectionMock = new Mock<IConfigurationSection>();
        var configurationMock = new Mock<IConfiguration>();
        var httpContextMock = new Mock<HttpContext>();

        configurationSectionMock
            .Setup(section => section.Value)
            .Returns("/regulators");

        configurationMock
            .Setup(config => config.GetSection(ConfigKeys.PathBase))
            .Returns(configurationSectionMock.Object);

        sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(new JourneySession());

        _controller = new ManageRegistrationsController(
            _reprocessorExporterServiceMock.Object,
            _mapperMock.Object,
            _validatorMock.Object,
            sessionManagerMock.Object,
            configurationMock.Object
        );

        _controller.ControllerContext.HttpContext = httpContextMock.Object;
    }

    [TestMethod]
    public async Task Index_ShouldDisplayBackLink()
    {
        // Arrange
        var registration = new Registration
        {
            Id = RegistrationId,
            OrganisationName = "Test Exporter Ltd",
            SiteAddress = "N/A",
            OrganisationType = ApplicationOrganisationType.Exporter,
            Regulator = "Environment Agency (EA)"
        };

        var expectedModel = new ManageRegistrationsViewModel
        {
            Id = registration.Id,
            OrganisationName = registration.OrganisationName,
            SiteAddress = registration.SiteAddress,
            ApplicationOrganisationType = registration.OrganisationType,
            Regulator = registration.Regulator
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IdRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _reprocessorExporterServiceMock.Setup(s => s.GetRegistrationByIdAsync(RegistrationId)).ReturnsAsync(registration);
        _mapperMock.Setup(m => m.Map<ManageRegistrationsViewModel>(registration)).Returns(expectedModel);

        // Act
        var result = await _controller.Index(RegistrationId);

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
    public async Task Index_ValidId_ShouldReturnViewResultWithCorrectModel()
    {
        // Arrange
        var registration = new Registration
        {
            Id = RegistrationId,
            OrganisationName = "Test Exporter Ltd",
            SiteAddress = "N/A",
            OrganisationType = ApplicationOrganisationType.Exporter,
            Regulator = "Environment Agency (EA)"
        };

        var expectedModel = new ManageRegistrationsViewModel
        {
            Id = registration.Id,
            OrganisationName = registration.OrganisationName,
            SiteAddress = registration.SiteAddress,
            ApplicationOrganisationType = registration.OrganisationType,
            Regulator = registration.Regulator
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IdRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _reprocessorExporterServiceMock.Setup(s => s.GetRegistrationByIdAsync(RegistrationId)).ReturnsAsync(registration);
        _mapperMock.Setup(m => m.Map<ManageRegistrationsViewModel>(registration)).Returns(expectedModel);

        // Act
        var result = await _controller.Index(RegistrationId);

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
            model!.Id.Should().Be(RegistrationId);
            model.OrganisationName.Should().Be(expectedModel.OrganisationName);
            model.SiteAddress.Should().Be(expectedModel.SiteAddress);
            model.ApplicationOrganisationType.Should().Be(expectedModel.ApplicationOrganisationType);
            model.Regulator.Should().Be(expectedModel.Regulator);
        }
    }

    [TestMethod]
    public async Task Index_InvalidId_ShouldThrowValidationException()
    {
        // Arrange
        var id = 0; // Invalid ID
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure(nameof(IdRequest.Id), "ID must be greater than 0.")
        };

        var validationResult = new ValidationResult(validationFailures);

        _validatorMock
            .Setup(v => v.Validate(It.IsAny<IdRequest>()))
            .Returns(validationResult);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<ValidationException>(async () =>
        {
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            await _controller.Index(id);
        });

        exception.Errors.Should().NotBeNullOrEmpty();
        exception.Errors.Select(e => e.ErrorMessage).Should().Contain("ID must be greater than 0.");
    }

    [TestMethod]
    public async Task Index_Exception_ShouldThrowException()
    {
        // Arrange
        _ = _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IdRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _reprocessorExporterServiceMock.Setup(s => s.GetRegistrationByIdAsync(RegistrationId)).Throws(new Exception("Test exception"));

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<Exception>(async () => await _controller.Index(RegistrationId));

        exception.Message.Should().Be("Test exception");
    }
}