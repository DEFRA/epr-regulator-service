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

        _journeySession = new JourneySession();

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
        var applicationUpdateSession = CreateApplicationUpdateSession(123);
        var expectedViewModel = new ApplicationUpdateViewModel();

        _journeySession.ReprocessorExporterSession.ApplicationUpdateSession = applicationUpdateSession;

        _mapperMock.Setup(m => m.Map<ApplicationUpdateViewModel>(applicationUpdateSession))
            .Returns(expectedViewModel);

        // Act
        var response = await _applicationUpdateController.Index(1);

        // Assert
        response.Should().BeOfType<ViewResult>();

        var viewResult = response as ViewResult;
        viewResult?.Model.Should().Be(expectedViewModel);
        viewResult?.ViewName.Should().EndWith("ApplicationUpdate.cshtml");
    }

    // TODO... complete the following tests...

    ////[TestMethod]
    ////public void Index_WhenCalledWithViewModelAndApplicationUpdateSessionIsNull__ShouldThrowException()
    ////{
    ////    Assert.Fail();
    ////}

    ////[TestMethod]
    ////public void Index_WhenCalledWithViewModelAndInvalidModelState_ShouldRedisplayView()
    ////{
    ////    Assert.Fail();
    ////}

    ////[TestMethod]
    ////[DataRow(ApplicationStatus.Granted, PagePath.ApplicationGrantedDetails)]
    ////[DataRow(ApplicationStatus.Refused, PagePath.ApplicationRefusedDetails)]
    ////public void Index_WhenCalledWithViewModelAndValidModel_ShouldRedirectToNextPage(ApplicationStatus status, string expectedPage)
    ////{
    ////    Assert.Fail();
    ////}

    ////[TestMethod]
    ////public void Index_WhenCalledWithViewModelAndValidModel_ShouldUpdateSessionWithStatus()
    ////{
    ////    Assert.Fail();
    ////}

    ////[TestMethod]
    ////public void ApplicationGrantedDetails_ShouldDisplayView()
    ////{
    ////    Assert.Fail();
    ////}

    ////[TestMethod]
    ////public void ApplicationRefusedDetails_ShouldDisplayView()
    ////{
    ////    Assert.Fail();
    ////}

    ////[TestMethod]
    ////public void SaveGrantedApplication_WhenModelStateIsInvalid_ShouldRedisplayView()
    ////{
    ////    Assert.Fail();
    ////}

    ////[TestMethod]
    ////public void SaveGrantedApplication_WhenModelStateIsValid_ShouldCallService()
    ////{
    ////    Assert.Fail();
    ////}

    ////[TestMethod]
    ////public void SaveGrantedApplication_WhenModelStateIsValid_ShouldRedirectToManageRegistrations()
    ////{
    ////    Assert.Fail();
    ////}

    ////[TestMethod]
    ////public void SaveRefusedApplication_WhenModelStateIsInvalid_ShouldRedisplayView()
    ////{
    ////    Assert.Fail();
    ////}

    ////[TestMethod]
    ////public void SaveRefusedApplication_WhenModelStateIsValid_ShouldCallService()
    ////{
    ////    Assert.Fail();
    ////}

    ////[TestMethod]
    ////public void SaveRefusedApplication_WhenModelStateIsValid_ShouldRedirectToManageRegistrations()
    ////{
    ////    Assert.Fail();
    ////}

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