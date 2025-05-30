using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.Sessions;

using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using EPR.RegulatorService.Frontend.Core.Exceptions;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using FluentAssertions.Execution;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.ApplicationUpdate;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers.ReprocessorExporter.Registrations;

[TestClass]
public class ApplicationUpdateControllerTests
{
    private ApplicationUpdateController _applicationUpdateController; // System under test

    private Mock<ISessionManager<JourneySession>> _sessionManagerMock;
    private Mock<IMapper> _mapperMock;
    private Mock<IReprocessorExporterService> _reprocessorExporterServiceMock;

    private JourneySession _journeySession;

    [TestInitialize]
    public void TestInitialize()
    {
        Guid registrationMaterialId = Guid.Parse("F267151B-07F0-43CE-BB5B-37671609EB21");

        _sessionManagerMock = new Mock<ISessionManager<JourneySession>>();
        _mapperMock = new Mock<IMapper>();
        _reprocessorExporterServiceMock = new Mock<IReprocessorExporterService>();

        var validatorMock = new Mock<IValidator<IdRequest>>();
        var configurationSectionMock = new Mock<IConfigurationSection>();
        var configurationMock = new Mock<IConfiguration>();
        
        configurationSectionMock
            .Setup(section => section.Value)
            .Returns("/regulators");

        configurationMock
            .Setup(config => config.GetSection(ConfigKeys.PathBase))
            .Returns(configurationSectionMock.Object);

        _journeySession = new JourneySession
        {
            ReprocessorExporterSession = { ApplicationUpdateSession = CreateApplicationUpdateSession(registrationMaterialId) }
        };

        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(_journeySession);

        _applicationUpdateController = new ApplicationUpdateController(
            _mapperMock.Object,
            validatorMock.Object,
            _reprocessorExporterServiceMock.Object,
            _sessionManagerMock.Object,
        configurationMock.Object );

        var httpContextMock = new Mock<HttpContext>();
        _applicationUpdateController.ControllerContext.HttpContext = httpContextMock.Object;
    }

    [TestMethod]
    public async Task ApplicationUpdate_WhenCalledWithIdAndSessionIsNull_ShouldThrowException()
    {
        // Arrange
        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!);

        // Act/Assert
        await Assert.ThrowsExceptionAsync<SessionException>(() => _applicationUpdateController.ApplicationUpdate(Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC")));
    }

    [TestMethod]
    public async Task ApplicationUpdate_WhenCalledWithIdAndApplicationUpdateSessionIsNull_ShouldCreateNewSessionFromRegistrationMaterialDetail()
    {
        // Arrange
        var registrationMaterial = CreateRegistrationMaterialDetail(Guid.Parse("F267151B-07F0-43CE-BB5B-37671609EB21"));

        _journeySession.ReprocessorExporterSession.ApplicationUpdateSession = null;

        _reprocessorExporterServiceMock.Setup(r => r.GetRegistrationMaterialByIdAsync(registrationMaterial.Id))
            .ReturnsAsync(registrationMaterial);

        _mapperMock.Setup(m => m.Map<ApplicationUpdateSession>(registrationMaterial))
            .Returns(CreateApplicationUpdateSession(registrationMaterial.Id));

        // Act
        await _applicationUpdateController.ApplicationUpdate(registrationMaterial.Id);

        // Assert
        _journeySession.ReprocessorExporterSession.ApplicationUpdateSession.Should().NotBeNull();
    }

    [TestMethod]
    public async Task ApplicationUpdate_WhenCalledWithIdAndValidSession_ShouldReturnViewResult()
    {
        // Arrange
        var expectedViewModel = new ApplicationUpdateViewModel();

        _mapperMock.Setup(m =>
                m.Map<ApplicationUpdateViewModel>(_journeySession.ReprocessorExporterSession.ApplicationUpdateSession))
            .Returns(expectedViewModel);

        // Act
        var response = await _applicationUpdateController.ApplicationUpdate(Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"));

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult)response;
            viewResult.Model.Should().Be(expectedViewModel);
            viewResult.ViewName.Should().EndWith("ApplicationUpdate.cshtml");
        }
    }

    [TestMethod]
    public async Task ApplicationUpdate_WhenCalledWithViewModelAndApplicationUpdateSessionIsNull__ShouldThrowException()
    {
        // Arrange
        var viewModel = new ApplicationUpdateViewModel();

        _journeySession.ReprocessorExporterSession.ApplicationUpdateSession = null;

        // Act/Assert
        await Assert.ThrowsExceptionAsync<SessionException>(() => _applicationUpdateController.ApplicationUpdate(viewModel));
    }

    [TestMethod]
    public async Task ApplicationUpdate_WhenCalledWithViewModelAndInvalidModelState_ShouldRedisplayView()
    {
        // Arrange
        var viewModel = new ApplicationUpdateViewModel();

        _applicationUpdateController.ModelState.AddModelError("Test", "Error");

        // Act
        var response = await _applicationUpdateController.ApplicationUpdate(viewModel);

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult)response;
            viewResult.ViewName.Should().EndWith("ApplicationUpdate.cshtml");
        }
    }

    [TestMethod]
    [DataRow(ApplicationStatus.Granted, PagePath.ApplicationGrantedDetails)]
    [DataRow(ApplicationStatus.Refused, PagePath.ApplicationRefusedDetails)]
    public async Task ApplicationUpdate_WhenCalledWithViewModelAndValidModel_ShouldRedirectToNextPage(ApplicationStatus status, string expectedAction)
    {
        // Arrange
        var viewModel = new ApplicationUpdateViewModel { Status = status };

        // Act
        var response = await _applicationUpdateController.ApplicationUpdate(viewModel);

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<RedirectToActionResult>();

            var redirectToActionResult = (RedirectToActionResult)response;
            redirectToActionResult.ActionName.Should().Be(expectedAction);
        }
    }

    [TestMethod]
    public async Task ApplicationUpdate_WhenCalledWithViewModelAndValidModel_ShouldUpdateSessionWithStatus()
    {
        // Arrange
        var viewModel = new ApplicationUpdateViewModel { Status = ApplicationStatus.Granted };

        _journeySession.ReprocessorExporterSession.ApplicationUpdateSession!.Status = null;

        // Act
        await _applicationUpdateController.ApplicationUpdate(viewModel);

        // Assert
        _journeySession.ReprocessorExporterSession.ApplicationUpdateSession!.Status.Should().Be(viewModel.Status);
    }

    [TestMethod]
    public async Task ApplicationGrantedDetails_ShouldDisplayView()
    {
        // Act
        var response = await _applicationUpdateController.ApplicationGrantedDetails();

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult)response;
            viewResult.ViewName.Should().EndWith("ApplicationGrantedDetails.cshtml");
        }
    }

    [TestMethod]
    public async Task ApplicationRefusedDetails_ShouldDisplayView()
    {
        // Act
        var response = await _applicationUpdateController.ApplicationRefusedDetails();

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult)response;
            viewResult.ViewName.Should().EndWith("ApplicationRefusedDetails.cshtml");
        }
    }

    [TestMethod]
    public async Task ApplicationGrantedDetails_WhenModelStateIsInvalid_ShouldRedisplayView()
    {
        // Arrange
        var viewModel = new ApplicationGrantedViewModel();

        _applicationUpdateController.ModelState.AddModelError("Test", "Error");

        // Act
        var response = await _applicationUpdateController.ApplicationGrantedDetails(viewModel);

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult)response;
            viewResult.ViewName.Should().EndWith("ApplicationGrantedDetails.cshtml");
        }
    }

    [TestMethod]
    public async Task ApplicationGrantedDetails_WhenModelStateIsValid_ShouldCallService()
    {
        // Arrange
        var viewModel = new ApplicationGrantedViewModel { Comments = "Test Comments" };

        var session = _journeySession.ReprocessorExporterSession.ApplicationUpdateSession;
        session!.Status = ApplicationStatus.Granted;

        // Act
        await _applicationUpdateController.ApplicationGrantedDetails(viewModel);

        // Assert
        _reprocessorExporterServiceMock.Verify(r => r.UpdateRegistrationMaterialOutcomeAsync(
            session.RegistrationMaterialId,
            It.Is<RegistrationMaterialOutcomeRequest>(req => req.Status == session.Status && req.Comments == viewModel.Comments)), Times.Once);
    }

    [TestMethod]
    public async Task ApplicationGrantedDetails_WhenModelStateIsValid_ShouldRedirectToManageRegistrations()
    {
        // Arrange
        var viewModel = new ApplicationGrantedViewModel();

        // Act
        var response = await _applicationUpdateController.ApplicationGrantedDetails(viewModel);

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<RedirectToActionResult>();

            var redirectToActionResult = (RedirectToActionResult)response;
            redirectToActionResult.ActionName.Should().Be(PagePath.ManageRegistrations);
            redirectToActionResult.ControllerName.Should().Be(PagePath.ReprocessorExporterRegistrations);
        }
    }

    [TestMethod]
    public async Task ApplicationRefusedDetails_WhenModelStateIsInvalid_ShouldRedisplayView()
    {
        // Arrange
        var viewModel = new ApplicationRefusedViewModel { Comments = "Test Comments" };

        _applicationUpdateController.ModelState.AddModelError("Test", "Error");

        // Act
        var response = await _applicationUpdateController.ApplicationRefusedDetails(viewModel);

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult)response;
            viewResult.ViewName.Should().EndWith("ApplicationRefusedDetails.cshtml");
        }
    }

    [TestMethod]
    public async Task ApplicationRefusedDetails_WhenModelStateIsValid_ShouldCallService()
    {
        // Arrange
        var viewModel = new ApplicationRefusedViewModel { Comments = "Test Comments" };

        var session = _journeySession.ReprocessorExporterSession.ApplicationUpdateSession;
        session!.Status = ApplicationStatus.Granted;

        // Act
        await _applicationUpdateController.ApplicationRefusedDetails(viewModel);

        // Assert
        _reprocessorExporterServiceMock.Verify(r => r.UpdateRegistrationMaterialOutcomeAsync(
            session.RegistrationMaterialId,
            It.Is<RegistrationMaterialOutcomeRequest>(req => req.Status == session.Status && req.Comments == viewModel.Comments)), Times.Once);
    }

    [TestMethod]
    public async Task ApplicationRefusedDetails_WhenModelStateIsValid_ShouldRedirectToManageRegistrations()
    {
        // Arrange
        var viewModel = new ApplicationRefusedViewModel { Comments = "Test Comments" };

        // Act
        var response = await _applicationUpdateController.ApplicationRefusedDetails(viewModel);

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<RedirectToActionResult>();

            var redirectToActionResult = (RedirectToActionResult)response;
            redirectToActionResult.ActionName.Should().Be(PagePath.ManageRegistrations);
            redirectToActionResult.ControllerName.Should().Be(PagePath.ReprocessorExporterRegistrations);
        }
    }

    private static RegistrationMaterialDetail CreateRegistrationMaterialDetail(Guid registrationMaterialId) =>
        new()
        {
            Id = registrationMaterialId,
            RegistrationId = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"),
            MaterialName = "Plastic"
        };

    private static ApplicationUpdateSession CreateApplicationUpdateSession(Guid registrationMaterialId) =>
        new()
        {
            RegistrationMaterialId = registrationMaterialId,
            RegistrationId = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"),
            MaterialName = "Plastic"
        };
}