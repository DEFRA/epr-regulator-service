using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

using FluentAssertions.Execution;

using FluentValidation;
using FluentValidation.Results;

using Microsoft.AspNetCore.Mvc;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers.ReprocessorExporter.Registrations;

[TestClass]
public class ManageRegistrationsControllerTests : RegistrationControllerTestBase
{
    private const string BackLinkViewDataKey = "BackLinkToDisplay";
    private readonly Guid _registrationId = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC");

    private ManageRegistrationsController _controller;
    private Mock<IValidator<IdRequest>> _validatorMock;

    [TestInitialize]
    public void TestInitialize()
    {
        CreateCommonMocks();
        CreateSessionMock();

        _validatorMock = new Mock<IValidator<IdRequest>>();
        var configurationMock = CreateConfigurationMock();

        _controller = new ManageRegistrationsController(
            _reprocessorExporterServiceMock.Object,
            _mapperMock.Object,
            _validatorMock.Object,
            _sessionManagerMock.Object,
            configurationMock.Object
        );

        _controller.ControllerContext.HttpContext = _httpContextMock.Object;
    }

    [TestMethod]
    public async Task Index_ShouldDisplayBackLink()
    {
        // Arrange
        var registration = new Registration
        {
            Id = _registrationId,
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

        _reprocessorExporterServiceMock.Setup(s => s.GetRegistrationByIdAsync(_registrationId)).ReturnsAsync(registration);
        _mapperMock.Setup(m => m.Map<ManageRegistrationsViewModel>(registration)).Returns(expectedModel);

        // Act
        var result = await _controller.Index(_registrationId);

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
            Id = _registrationId,
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

        _reprocessorExporterServiceMock.Setup(s => s.GetRegistrationByIdAsync(_registrationId)).ReturnsAsync(registration);
        _mapperMock.Setup(m => m.Map<ManageRegistrationsViewModel>(registration)).Returns(expectedModel);

        // Act
        var result = await _controller.Index(_registrationId);

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
            model!.Id.Should().Be(_registrationId);
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
        var id = Guid.Empty; // Invalid ID
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

        _reprocessorExporterServiceMock.Setup(s => s.GetRegistrationByIdAsync(_registrationId)).Throws(new Exception("Test exception"));

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<Exception>(async () => await _controller.Index(_registrationId));

        exception.Message.Should().Be("Test exception");
    }
}