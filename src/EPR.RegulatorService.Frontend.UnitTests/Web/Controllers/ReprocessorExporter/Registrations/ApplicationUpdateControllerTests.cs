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

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers.ReprocessorExporter.Registrations;

[TestClass]
public class ApplicationUpdateControllerTests
{
    private ApplicationUpdateController _applicationUpdateController; // System under test

    private Mock<ISessionManager<JourneySession>> _sessionManagerMock;
    private Mock<IMapper> _mapperMock;
    private Mock<IRegistrationService> _registrationServiceMock;

    private JourneySession _journeySession;

    [TestInitialize]
    public void TestInitialize()
    {
        const int registrationMaterialId = 123;

        _sessionManagerMock = new Mock<ISessionManager<JourneySession>>();
        _mapperMock = new Mock<IMapper>();
        _registrationServiceMock = new Mock<IRegistrationService>();

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
            _registrationServiceMock.Object,
            _sessionManagerMock.Object,
        configurationMock.Object );

        var httpContextMock = new Mock<HttpContext>();
        _applicationUpdateController.ControllerContext.HttpContext = httpContextMock.Object;
    }

    [TestMethod]
    public async Task Index_WhenCalledWithIdAndSessionIsNull_ShouldThrowException()
    {
        // Arrange
        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!);

        // Act/Assert
        await Assert.ThrowsExceptionAsync<SessionException>(() => _applicationUpdateController.Index(1));
    }

    [TestMethod]
    public async Task Index_WhenCalledWithIdAndApplicationUpdateSessionIsNull_ShouldCreateNewSessionFromRegistrationMaterial()
    {
        // Arrange
        var registrationMaterial = CreateRegistrationMaterial(123);

        _journeySession.ReprocessorExporterSession.ApplicationUpdateSession = null;

        _registrationServiceMock.Setup(r => r.GetRegistrationMaterialAsync(registrationMaterial.Id))
            .ReturnsAsync(registrationMaterial);

        _mapperMock.Setup(m => m.Map<ApplicationUpdateSession>(registrationMaterial))
            .Returns(CreateApplicationUpdateSession(registrationMaterial.Id));

        // Act
        await _applicationUpdateController.Index(registrationMaterial.Id);

        // Assert
        _journeySession.ReprocessorExporterSession.ApplicationUpdateSession.Should().NotBeNull();
    }

    [TestMethod]
    public async Task Index_WhenCalledWithIdAndValidSession_ShouldReturnViewResult()
    {
        // Arrange
        var expectedViewModel = new ApplicationUpdateViewModel();

        _mapperMock.Setup(m => m.Map<ApplicationUpdateViewModel>(_journeySession.ReprocessorExporterSession.ApplicationUpdateSession))
            .Returns(expectedViewModel);

        // Act
        var response = await _applicationUpdateController.Index(1);

        // Assert
        response.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)response;
        viewResult.Model.Should().Be(expectedViewModel);
        viewResult.ViewName.Should().EndWith("ApplicationUpdate.cshtml");
    }

    [TestMethod]
    public async Task Index_WhenCalledWithViewModelAndApplicationUpdateSessionIsNull__ShouldThrowException()
    {
        // Arrange
        var viewModel = new ApplicationUpdateViewModel();

        _journeySession.ReprocessorExporterSession.ApplicationUpdateSession = null;

        // Act/Assert
        await Assert.ThrowsExceptionAsync<SessionException>(() => _applicationUpdateController.Index(viewModel));
    }

    [TestMethod]
    public async Task Index_WhenCalledWithViewModelAndInvalidModelState_ShouldRedisplayView()
    {
        // Arrange
        var viewModel = new ApplicationUpdateViewModel();

        _applicationUpdateController.ModelState.AddModelError("Test", "Error");

        // Act
        var response = await _applicationUpdateController.Index(viewModel);

        // Assert
        response.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)response;
        viewResult.ViewName.Should().EndWith("ApplicationUpdate.cshtml");
    }

    [TestMethod]
    [DataRow(ApplicationStatus.Granted, PagePath.ApplicationGrantedDetails)]
    [DataRow(ApplicationStatus.Refused, PagePath.ApplicationRefusedDetails)]
    public async Task Index_WhenCalledWithViewModelAndValidModel_ShouldRedirectToNextPage(ApplicationStatus status, string expectedAction)
    {
        // Arrange
        var viewModel = new ApplicationUpdateViewModel { Status = status };

        // Act
        var response = await _applicationUpdateController.Index(viewModel);

        // Assert
        response.Should().BeOfType<RedirectToActionResult>();

        var redirectToActionResult = (RedirectToActionResult)response;
        redirectToActionResult.ActionName.Should().Be(expectedAction);
    }

    [TestMethod]
    public async Task Index_WhenCalledWithViewModelAndValidModel_ShouldUpdateSessionWithStatus()
    {
        // Arrange
        var viewModel = new ApplicationUpdateViewModel { Status = ApplicationStatus.Granted };

        _journeySession.ReprocessorExporterSession.ApplicationUpdateSession!.Status = null;

        // Act
        await _applicationUpdateController.Index(viewModel);

        // Assert
        _journeySession.ReprocessorExporterSession.ApplicationUpdateSession!.Status.Should().Be(viewModel.Status);
    }

    [TestMethod]
    public async Task ApplicationGrantedDetails_ShouldDisplayView()
    {
        // Act
        var response = await _applicationUpdateController.ApplicationGrantedDetails();

        // Assert
        response.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)response;
        viewResult.ViewName.Should().EndWith("ApplicationGrantedDetails.cshtml");
    }

    [TestMethod]
    public async Task ApplicationRefusedDetails_ShouldDisplayView()
    {
        // Act
        var response = await _applicationUpdateController.ApplicationRefusedDetails();

        // Assert
        response.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)response;
        viewResult.ViewName.Should().EndWith("ApplicationRefusedDetails.cshtml");
    }

    //[TestMethod]
    //public void SaveGrantedApplication_WhenModelStateIsInvalid_ShouldRedisplayView()
    //{
    //    Assert.Fail();
    //}

    //[TestMethod]
    //public void SaveGrantedApplication_WhenModelStateIsValid_ShouldCallService()
    //{
    //    Assert.Fail();
    //}

    //[TestMethod]
    //public void SaveGrantedApplication_WhenModelStateIsValid_ShouldRedirectToManageRegistrations()
    //{
    //    Assert.Fail();
    //}

    //[TestMethod]
    //public void SaveRefusedApplication_WhenModelStateIsInvalid_ShouldRedisplayView()
    //{
    //    Assert.Fail();
    //}

    //[TestMethod]
    //public void SaveRefusedApplication_WhenModelStateIsValid_ShouldCallService()
    //{
    //    Assert.Fail();
    //}

    //[TestMethod]
    //public void SaveRefusedApplication_WhenModelStateIsValid_ShouldRedirectToManageRegistrations()
    //{
    //    Assert.Fail();
    //}

    private static RegistrationMaterial CreateRegistrationMaterial(int registrationMaterialId) =>
        new()
        {
            Id = registrationMaterialId,
            RegistrationId = 1,
            MaterialName = "Plastic",
            DeterminationDate = DateTime.UtcNow,
            Status = ApplicationStatus.Granted,
            StatusUpdatedByName = "Test User",
            StatusUpdatedAt = DateTime.UtcNow,
            RegistrationNumber = "REG123",
            Tasks = []
        };

    private static ApplicationUpdateSession CreateApplicationUpdateSession(int registrationMaterialId) =>
        new()
        {
            RegistrationMaterialId = registrationMaterialId,
            RegistrationId = 1,
            MaterialName = "Plastic",
            Status = ApplicationStatus.Granted
        };
}